//using M3UEditor.Data;
using M3UManager.Models.Commands;
using M3UManager.Services.FavoriteServices;
using M3UManager.Services.FileIOServices;
using M3UManager.Services.M3UEditorCommands;
using M3UManager.Services.M3UListServices;

namespace M3UManager.Services.Config
{
    public static class ServicesExententions
    {
        public static void RegisterDIServices(this IServiceCollection services)
        {
            //services.AddDbContext<AppDbContext>();
            services.AddSingleton<IFavoritesService, FavoritesService>();
            services.AddSingleton<IEditorService, EditorService>();
            services.AddSingleton<ICommandFactory, M3UEditorCommandsFactory>();
            services.AddTransient<IFileIOService, FileIO>();
        }
    }
}
