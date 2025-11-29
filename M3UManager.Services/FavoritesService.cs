using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using Microsoft.Maui.Storage;
using System.Text.Json;


namespace M3UManager.Services
{
    public class FavoritesService : IFavoritesService
    {
        public IFileIOService fileIOService { get; private set; }
        public string[] SelectedGroups { get; set; } = new string[0];
        public M3UGroupList FavoritesGroupList { get; private set; }
        private bool initialzed = false;
        private string favoritePath = FileSystem.AppDataDirectory + "/favorites.json";
        private string categoriesPath = FileSystem.AppDataDirectory + "/favorite_categories.json";
        private List<FavoriteCategory> _categories = new List<FavoriteCategory>();

        public FavoritesService(IFileIOService fileIOService)
        {
            this.fileIOService = fileIOService;
            InitializeDefaultCategories();
        }

        private void InitializeDefaultCategories()
        {
            // Load categories from file or create defaults
            if (File.Exists(categoriesPath))
            {
                try
                {
                    var json = File.ReadAllText(categoriesPath);
                    var categories = JsonSerializer.Deserialize<List<FavoriteCategory>>(json);
                    if (categories != null && categories.Count > 0)
                    {
                        _categories = categories;
                        return;
                    }
                }
                catch
                {
                    // If loading fails, create defaults
                }
            }

            // Create default categories
            _categories = new List<FavoriteCategory>
            {
                new FavoriteCategory("All", "bi-star-fill", true, 0),
                new FavoriteCategory("Sports", "bi-trophy-fill", true, 1),
                new FavoriteCategory("News", "bi-newspaper", true, 2),
                new FavoriteCategory("Movies", "bi-film", true, 3)
            };
            SaveCategories();
        }

        public void InitFavorites()
        {
            if (!initialzed)
            {
                try
                {
                    var favoriesStringList = File.ReadAllText(favoritePath);
                    FavoritesGroupList = new M3UGroupList(favoriesStringList);
                    
                    // Migrate old favorites to "All" category if needed
                    MigrateOldFavorites();
                    
                    initialzed = true;
                }
                catch
                {
                    FavoritesGroupList = new M3UGroupList { M3UGroups = new Dictionary<string, M3UGroup>() };
                }
            }
        }

        private void MigrateOldFavorites()
        {
            // Get "All" category
            var allCategory = _categories.FirstOrDefault(c => c.Name == "All");
            if (allCategory == null || allCategory.Channels.Count > 0)
                return; // Already migrated or no "All" category

            // Migrate all channels from old format to "All" category
            if (FavoritesGroupList?.M3UGroups != null)
            {
                foreach (var group in FavoritesGroupList.M3UGroups.Values)
                {
                    foreach (var channel in group.Channels)
                    {
                        allCategory.AddChannel(channel);
                    }
                }
                SaveCategories();
            }
        }

        public void SaveFavoritesListString() => fileIOService.SaveDictionaryAsM3U(FavoritesGroupList.M3UGroups, favoritePath);
        
        public bool IsChannelInFavorites(M3UChannel channel) =>
            _categories.Any(cat => cat.ContainsChannel(channel));

        public void AddChannelToFavory(M3UChannel channel)
        {
            // Add to "All" category by default
            var allCategory = _categories.FirstOrDefault(c => c.Name == "All");
            if (allCategory != null)
            {
                allCategory.AddChannel(channel);
                SaveCategories();
            }

            // Legacy: Also maintain old structure
            var key = Utils.TrimmedString(channel.Group);
            if (!FavoritesGroupList.M3UGroups.ContainsKey(key))
                FavoritesGroupList.M3UGroups[key] = new M3UGroup(channel.Group, new List<M3UChannel>());
            FavoritesGroupList.M3UGroups[key].AddChannel(channel);
        }

        public void RemoveChannelFromFavorites(M3UChannel channel)
        {
            // Remove from all categories
            foreach (var category in _categories)
            {
                category.RemoveChannel(channel.FullChannelString);
            }
            SaveCategories();

            // Legacy: Also remove from old structure
            var key = Utils.TrimmedString(channel.Group);
            if (FavoritesGroupList.M3UGroups.ContainsKey(key))
            {
                FavoritesGroupList.M3UGroups[key].RemoveChannel(channel.FullChannelString);
            }
        }

        // New category-based methods
        public List<FavoriteCategory> GetCategories() => _categories.OrderBy(c => c.SortOrder).ToList();

        public FavoriteCategory? GetCategory(string categoryId) => 
            _categories.FirstOrDefault(c => c.Id == categoryId);

        public FavoriteCategory AddCategory(string name, string icon = "bi-star")
        {
            var maxOrder = _categories.Any() ? _categories.Max(c => c.SortOrder) : -1;
            var newCategory = new FavoriteCategory(name, icon, false, maxOrder + 1);
            _categories.Add(newCategory);
            SaveCategories();
            return newCategory;
        }

        public void RemoveCategory(string categoryId)
        {
            var category = _categories.FirstOrDefault(c => c.Id == categoryId);
            if (category != null && !category.IsDefault)
            {
                // Move channels to "All" category before deleting
                var allCategory = _categories.FirstOrDefault(c => c.Name == "All");
                if (allCategory != null)
                {
                    foreach (var channel in category.Channels)
                    {
                        allCategory.AddChannel(channel);
                    }
                }
                
                _categories.Remove(category);
                SaveCategories();
            }
        }

        public void RenameCategory(string categoryId, string newName)
        {
            var category = _categories.FirstOrDefault(c => c.Id == categoryId);
            if (category != null && !category.IsDefault)
            {
                category.Name = newName;
                SaveCategories();
            }
        }

        public void UpdateCategoryIcon(string categoryId, string icon)
        {
            var category = _categories.FirstOrDefault(c => c.Id == categoryId);
            if (category != null)
            {
                category.Icon = icon;
                SaveCategories();
            }
        }

        public void ReorderCategories(List<string> categoryIds)
        {
            for (int i = 0; i < categoryIds.Count; i++)
            {
                var category = _categories.FirstOrDefault(c => c.Id == categoryIds[i]);
                if (category != null)
                {
                    category.SortOrder = i;
                }
            }
            SaveCategories();
        }

        public void AddChannelToCategory(string categoryId, M3UChannel channel)
        {
            var category = _categories.FirstOrDefault(c => c.Id == categoryId);
            if (category != null)
            {
                category.AddChannel(channel);
                
                // Also add to "All" category if not already there
                var allCategory = _categories.FirstOrDefault(c => c.Name == "All");
                if (allCategory != null && category.Id != allCategory.Id)
                {
                    allCategory.AddChannel(channel);
                }
                
                SaveCategories();
            }
        }

        public void RemoveChannelFromCategory(string categoryId, M3UChannel channel)
        {
            var category = _categories.FirstOrDefault(c => c.Id == categoryId);
            if (category != null)
            {
                category.RemoveChannel(channel.FullChannelString);
                
                // If removing from a specific category, check if it exists in other categories
                // If not, also remove from "All"
                if (category.Name != "All")
                {
                    var existsInOther = _categories
                        .Where(c => c.Name != "All" && c.Id != categoryId)
                        .Any(c => c.ContainsChannel(channel));
                    
                    if (!existsInOther)
                    {
                        var allCategory = _categories.FirstOrDefault(c => c.Name == "All");
                        allCategory?.RemoveChannel(channel.FullChannelString);
                    }
                }
                
                SaveCategories();
            }
        }

        public void MoveChannelToCategory(M3UChannel channel, string fromCategoryId, string toCategoryId)
        {
            var fromCategory = _categories.FirstOrDefault(c => c.Id == fromCategoryId);
            var toCategory = _categories.FirstOrDefault(c => c.Id == toCategoryId);
            
            if (fromCategory != null && toCategory != null)
            {
                fromCategory.RemoveChannel(channel.FullChannelString);
                toCategory.AddChannel(channel);
                SaveCategories();
            }
        }

        public bool IsChannelInCategory(string categoryId, M3UChannel channel)
        {
            var category = _categories.FirstOrDefault(c => c.Id == categoryId);
            return category?.ContainsChannel(channel) ?? false;
        }

        public void SaveCategories()
        {
            try
            {
                var json = JsonSerializer.Serialize(_categories, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(categoriesPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving categories: {ex.Message}");
            }
        }
    }
}
