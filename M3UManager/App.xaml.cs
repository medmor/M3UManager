using M3UManager.Services.ServicesContracts;
using M3UManager.Views;
using System.Runtime.InteropServices;

namespace M3UManager;

public partial class App : Application
{
    private readonly IFavoritesService favoritesService;
    private readonly IMediaPlayerService mediaPlayerService;
    private Window? currentPipWindow;
    private readonly List<Window> currentPlayerWindows = new();
    
    public App(IFavoritesService favoritesService, IMediaPlayerService mediaPlayerService)
    {
        InitializeComponent();

        MainPage = new MainPage();

        this.favoritesService = favoritesService;
        this.mediaPlayerService = mediaPlayerService;
        
        // Register the player window factory
        mediaPlayerService.RegisterWindowFactory(async (streamUrl, channelName) =>
        {
            // Close any existing PiP window when opening normal player
            if (currentPipWindow != null)
            {
                // Cleanup the PiP window before closing
                if (currentPipWindow.Page is PipWindow oldPipWindow)
                {
                    oldPipWindow.Cleanup();
                }
                Application.Current?.CloseWindow(currentPipWindow);
                currentPipWindow = null;
            }
            
            // Close all other player windows
            foreach (var playerWin in currentPlayerWindows.ToList())
            {
                Application.Current?.CloseWindow(playerWin);
            }
            currentPlayerWindows.Clear();
            
            var playerWindow = new PlayerWindow(streamUrl, channelName);
            
            // Create a new independent window
            var newWindow = new Window(playerWindow)
            {
                Title = "Media Player",
                Width = 850,
                Height = 650,
                X = 100,
                Y = 100
            };
            
            // Track the window
            currentPlayerWindows.Add(newWindow);
            newWindow.Destroying += (s, e) => currentPlayerWindows.Remove(newWindow);
            
            Application.Current?.OpenWindow(newWindow);
            
            await Task.CompletedTask;
        });

        // Register the PiP player factory - creates system-level floating window
        mediaPlayerService.RegisterPipFactory(async (streamUrl, channelName) =>
        {
            // Close all other player windows when opening PiP
            foreach (var playerWin in currentPlayerWindows.ToList())
            {
                Application.Current?.CloseWindow(playerWin);
            }
            currentPlayerWindows.Clear();
            
            // Close any existing PiP window first and cleanup
            if (currentPipWindow != null)
            {
                // Cleanup the PiP window before closing
                if (currentPipWindow.Page is PipWindow oldPipWindow)
                {
                    oldPipWindow.Cleanup();
                }
                Application.Current?.CloseWindow(currentPipWindow);
                currentPipWindow = null;
            }
            
            var pipWindow = new PipWindow();
            pipWindow.LoadStream(streamUrl, channelName);
            
            // Create a compact window for PIP
            var pipWin = new Window(pipWindow)
            {
                Title = channelName,
                Width = 400,
                Height = 280,
                X = 100,
                Y = 100
            };
            
            // Handle expand request - open full player and close PIP
            pipWindow.ExpandRequested += async (s, e) =>
            {
                // Close PiP when expanding
                if (currentPipWindow != null)
                {
                    // Cleanup before closing
                    if (currentPipWindow.Page is PipWindow closingPipWindow)
                    {
                        closingPipWindow.Cleanup();
                    }
                    Application.Current?.CloseWindow(currentPipWindow);
                    currentPipWindow = null;
                }
                
                var playerWindow = new PlayerWindow(streamUrl, channelName);
                var fullWindow = new Window(playerWindow)
                {
                    Title = "Media Player",
                    Width = 850,
                    Height = 650
                };
                
                // Track the new player window
                currentPlayerWindows.Add(fullWindow);
                fullWindow.Destroying += (s, e) => currentPlayerWindows.Remove(fullWindow);
                
                Application.Current?.OpenWindow(fullWindow);
            };
            
            // Track the PiP window
            currentPipWindow = pipWin;
            
            // Clear reference and cleanup when window is destroyed
            pipWin.Destroying += (s, e) =>
            {
                pipWindow.Cleanup();
                currentPipWindow = null;
            };
            
            Application.Current?.OpenWindow(pipWin);
            
            // Set always on top for Windows platform - multiple attempts to ensure it works
            pipWin.Created += async (s, e) =>
            {
                await Task.Delay(100);
                SetWindowAlwaysOnTop(pipWin);
                
                // Try again after another delay
                await Task.Delay(200);
                SetWindowAlwaysOnTop(pipWin);
            };
            
            // Also set on activated to maintain topmost status
            pipWin.Activated += (s, e) => SetWindowAlwaysOnTop(pipWin);
            
            // Reapply topmost when main window gets focus to ensure PiP stays above it
            if (Windows.Count > 0)
            {
                var mainWindow = Windows[0];
                mainWindow.Activated += (s, e) =>
                {
                    // Small delay then reapply topmost to PiP
                    Task.Run(async () =>
                    {
                        await Task.Delay(50);
                        MainThread.BeginInvokeOnMainThread(() => SetWindowAlwaysOnTop(pipWin));
                    });
                };
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
            
            // Close all player windows when main window closes
            foreach (var playerWin in currentPlayerWindows.ToList())
            {
                Application.Current?.CloseWindow(playerWin);
            }
            currentPlayerWindows.Clear();
        };

        return window;
    }
}
