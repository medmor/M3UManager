using M3UManager.Models;

namespace M3UManager.Services.ServicesContracts
{
    public interface IFavoritesService
    {
        IFileIOService fileIOService { get; }
        string[] SelectedGroups { get; set; }

        M3UGroupsList FavoritesGroupList { get; }
        void InitFavorites();
        void SaveFavoritesListString();
        bool IsChannelInFavorites(M3UChannel channel);
        void AddChannelToFavory(M3UChannel channel);
        void RemoveChannelFromFavorites(M3UChannel channel);
    }
}
