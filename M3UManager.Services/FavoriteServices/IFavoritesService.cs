using M3UManager.Models;
using M3UManager.Services.FileIOServices;

namespace M3UManager.Services.FavoriteServices
{
    public interface IFavoritesService
    {
        IFileIOService fileIOService { get; }
        string[] SelectedGroups { get; set; }

        M3UList FavoritesGroupList { get; }
        void InitFavorites();
        void SaveFavoritesListString();
        bool IsChannelInFavorites(M3UChannel channel);
        void AddChannelToFavory(M3UChannel channel);
        void RemoveChannelFromFavorites(M3UChannel channel);
    }
}
