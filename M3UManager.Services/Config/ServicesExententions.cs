﻿//using M3UEditor.Data;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.Config
{
    public static class ServicesExententions
    {
        public static void RegisterDIServices(this IServiceCollection services)
        {
            //services.AddDbContext<AppDbContext>();
            services.AddSingleton<IFavoritesService, FavoritesService>();
            services.AddSingleton<EditorService, EditorService>();
            services.AddSingleton<IFileIOService, FileIO>();
        }
    }
}
