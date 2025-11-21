using M3UManager.Services.ServicesContracts;
using M3UManager.Views;

namespace M3UManager;

public partial class App : Application
{
    private readonly IFavoritesService favoritesService;
    private readonly IMediaPlayerService mediaPlayerService;
    
    public App(IFavoritesService favoritesService, IMediaPlayerService mediaPlayerService)
    {
        InitializeComponent();

        MainPage = new MainPage();

        this.favoritesService = favoritesService;
        this.mediaPlayerService = mediaPlayerService;
        
        // Register the player window factory to create independent windows
        mediaPlayerService.RegisterWindowFactory(async (streamUrl, channelName) =>
        {
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
            
            Application.Current?.OpenWindow(newWindow);
            
            await Task.CompletedTask;
        });
    }
    
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
        };

        return window;
    }
}
