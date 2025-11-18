using M3UManager.Services.Config;
using CommunityToolkit.Maui;
#if WINDOWS
using Microsoft.AspNetCore.Components.WebView.Maui;
using M3UManager.Platforms.Windows;
#endif

namespace M3UManager;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

#if WINDOWS
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<BlazorWebView, CustomBlazorWebViewHandler>();
        });
#endif

        builder.Services.AddMauiBlazorWebView();
        builder.Services.RegisterDIServices();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif
        var app = builder.Build();
        //app.Services.AddUntaggedFavory();
        return app;
    }
}
