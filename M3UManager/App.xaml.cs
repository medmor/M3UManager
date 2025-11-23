using M3UManager.Services.ServicesContracts;
using M3UManager.Views;
using System.Runtime.InteropServices;

namespace M3UManager;

public partial class App : Application
{
    private readonly IFavoritesService favoritesService;
    private readonly IMediaPlayerService mediaPlayerService;
    private Window? currentPipWindow;
    private PipWindow? currentPipPage;
    private Window? currentPlayerWindow;
    private PlayerWindow? currentPlayerPage;
    
    public App(IFavoritesService favoritesService, IMediaPlayerService mediaPlayerService)
    {
        InitializeComponent();

        MainPage = new MainPage();

        this.favoritesService = favoritesService;
        this.mediaPlayerService = mediaPlayerService;
        
        // Register the player window factory - reuses existing window
        mediaPlayerService.RegisterWindowFactory(async (streamUrl, channelName) =>
        {
            // Close PiP if it's open
            if (currentPipWindow != null)
            {
                currentPipPage?.Cleanup();
                Application.Current?.CloseWindow(currentPipWindow);
                currentPipWindow = null;
                currentPipPage = null;
            }
            
            // Reuse existing player window or create new one
            if (currentPlayerWindow != null && currentPlayerPage != null)
            {
                // Update existing window with new stream
                currentPlayerPage.UpdateStream(streamUrl, channelName);
                currentPlayerWindow.Title = channelName;
                
                // Bring window to front
                if (currentPlayerWindow.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
                {
                    nativeWindow.Activate();
                }
            }
            else
            {
                // Create new player window
                currentPlayerPage = new PlayerWindow(streamUrl, channelName);
                currentPlayerWindow = new Window(currentPlayerPage)
                {
                    Title = channelName,
                    Width = 850,
                    Height = 650,
                    X = 100,
                    Y = 100
                };
                
                currentPlayerWindow.Destroying += (s, e) =>
                {
                    currentPlayerWindow = null;
                    currentPlayerPage = null;
                };
                
                Application.Current?.OpenWindow(currentPlayerWindow);
            }
            
            await Task.CompletedTask;
        });

        // Register the PiP player factory - reuses existing window
        mediaPlayerService.RegisterPipFactory(async (streamUrl, channelName) =>
        {
            // Close normal player if it's open
            if (currentPlayerWindow != null)
            {
                Application.Current?.CloseWindow(currentPlayerWindow);
                currentPlayerWindow = null;
                currentPlayerPage = null;
            }
            
            // Reuse existing PiP window or create new one
            if (currentPipWindow != null && currentPipPage != null)
            {
                // Update existing PiP with new stream
                currentPipPage.UpdateStream(streamUrl, channelName);
                currentPipWindow.Title = channelName;
                
                // Bring window to front and ensure it's topmost
                if (currentPipWindow.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
                {
                    nativeWindow.Activate();
                }
                SetWindowAlwaysOnTop(currentPipWindow);
            }
            else
            {
                // Create new PiP window
                currentPipPage = new PipWindow();
                currentPipPage.LoadStream(streamUrl, channelName);
                
                currentPipWindow = new Window(currentPipPage)
                {
                    Title = channelName,
                    Width = 400,
                    Height = 280,
                    X = 100,
                    Y = 100
                };
                
                // Handle expand request - open full player and close PIP
                currentPipPage.ExpandRequested += async (s, e) =>
                {
                    if (currentPipWindow != null)
                    {
                        currentPipPage?.Cleanup();
                        Application.Current?.CloseWindow(currentPipWindow);
                        currentPipWindow = null;
                        currentPipPage = null;
                    }
                    
                    // Open in normal player instead
                    await mediaPlayerService.OpenPlayerWindow(streamUrl, channelName);
                };
                
                currentPipWindow.Destroying += (s, e) =>
                {
                    currentPipPage?.Cleanup();
                    currentPipWindow = null;
                    currentPipPage = null;
                };
                
                Application.Current?.OpenWindow(currentPipWindow);
                
                // Set always on top - multiple attempts
                currentPipWindow.Created += async (s, e) =>
                {
                    await Task.Delay(100);
                    SetWindowAlwaysOnTop(currentPipWindow);
                    
                    await Task.Delay(200);
                    SetWindowAlwaysOnTop(currentPipWindow);
                };
                
                // Maintain topmost status
                currentPipWindow.Activated += (s, e) => SetWindowAlwaysOnTop(currentPipWindow);
                
                // Reapply topmost when main window gets focus
                if (Windows.Count > 0)
                {
                    var mainWindow = Windows[0];
                    mainWindow.Activated += (s, e) =>
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(50);
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                if (currentPipWindow != null)
                                {
                                    SetWindowAlwaysOnTop(currentPipWindow);
                                }
                            });
                        });
                    };
                }
            }
            
            await Task.CompletedTask;
        });
    }
    
    private void SetWindowAlwaysOnTop(Window window)
    {
#if WINDOWS
        try
        {
            var nativeWindow = window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (nativeWindow == null)
            {
                System.Diagnostics.Debug.WriteLine("Native window is null, handler not ready");
                return;
            }
            
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            if (hwnd == IntPtr.Zero)
            {
                System.Diagnostics.Debug.WriteLine("HWND is zero");
                return;
            }
            
            const int HWND_TOPMOST = -1;
            const uint SWP_NOMOVE = 0x0002;
            const uint SWP_NOSIZE = 0x0001;
            const uint SWP_NOACTIVATE = 0x0010;
            
            // Set window to always on top without activating it
            bool success = SetWindowPos(
                hwnd,
                (IntPtr)HWND_TOPMOST,
                0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            
            if (success)
            {
                System.Diagnostics.Debug.WriteLine("✓ PiP window set to always on top");
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                System.Diagnostics.Debug.WriteLine($"✗ SetWindowPos failed with error: {error}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting window always on top: {ex.Message}");
        }
#endif
    }

#if WINDOWS
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
#endif
    
    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = base.CreateWindow(activationState);
        window.Created += (s, e) =>
        {
            favoritesService.InitFavorites();
        };
        window.Destroying += (s, e) =>
        {
            favoritesService.SaveFavoritesListString();
            
            // Close PiP window when main window closes
            if (currentPipWindow != null)
            {
                if (currentPipWindow.Page is PipWindow pipWindow)
                {
                    pipWindow.Cleanup();
                }
                Application.Current?.CloseWindow(currentPipWindow);
                currentPipWindow = null;
            }
            
            // Close player window when main window closes
            if (currentPlayerWindow != null)
            {
                if (currentPlayerPage != null)
                {
                    currentPlayerPage.Cleanup();
                }
                Application.Current?.CloseWindow(currentPlayerWindow);
                currentPlayerWindow = null;
                currentPlayerPage = null;
            }
        };

        return window;
    }
}
