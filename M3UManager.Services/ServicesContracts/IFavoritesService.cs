using M3UManager.Models;

namespace M3UManager.Services.ServicesContracts
{
    public interface IFavoritesService
    {
        IFileIOService fileIOService { get; }
        string[] SelectedGroups { get; set; }

        M3UGroupList FavoritesGroupList { get; }
        
        // Legacy favorites methods (kept for backward compatibility)
        void InitFavorites();
        void SaveFavoritesListString();
        bool IsChannelInFavorites(M3UChannel channel);
        void AddChannelToFavory(M3UChannel channel);
        void RemoveChannelFromFavorites(M3UChannel channel);

        // New category-based methods
        List<FavoriteCategory> GetCategories();
        FavoriteCategory? GetCategory(string categoryId);
        FavoriteCategory AddCategory(string name, string icon = "bi-star");
        void RemoveCategory(string categoryId);
        void RenameCategory(string categoryId, string newName);
        void UpdateCategoryIcon(string categoryId, string icon);
        void ReorderCategories(List<string> categoryIds);
        void AddChannelToCategory(string categoryId, M3UChannel channel);
        void RemoveChannelFromCategory(string categoryId, M3UChannel channel);
        void MoveChannelToCategory(M3UChannel channel, string fromCategoryId, string toCategoryId);
        bool IsChannelInCategory(string categoryId, M3UChannel channel);
        void SaveCategories();
    }
}
