//using M3UEditor.Data;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.Config
{
    public static class ServicesExententions
    {
        public static void RegisterDIServices(this IServiceCollection services)
        {
            //services.AddDbContext<AppDbContext>();
            services.AddSingleton<IFavoritesService, FavoritesService>();
            
            // Configure HttpClient with timeout for XtreamService
            services.AddHttpClient<IXtreamService, XtreamService>()
                .ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30); // 30 second timeout
                });
            
            services.AddSingleton<IM3UService, M3UService>();
            services.AddSingleton<IFileIOService, FileIO>();
            services.AddSingleton<IMediaPlayerService, MediaPlayerService>();
            services.AddSingleton<IWatchHistoryService, WatchHistoryService>();
        }
    }
}
