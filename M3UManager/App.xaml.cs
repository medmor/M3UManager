using M3UManager.Services.ServicesContracts;

namespace M3UManager;

public partial class App : Application
{
    private readonly IFavoritesService favoritesService;
    public App(IFavoritesService favoritesService)
    {
        InitializeComponent();

        MainPage = new MainPage();

        this.favoritesService = favoritesService;
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
