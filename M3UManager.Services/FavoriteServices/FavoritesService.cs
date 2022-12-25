using M3UManager.Models;
using M3UManager.Services.FileIOServices;


namespace M3UManager.Services.FavoriteServices
{
    public class FavoritesService : IFavoritesService
    {
        public IFileIOService fileIOService { get; private set; }
        public string[] SelectedGroups { get; set; } = new string[0];
        public M3UList FavoritesGroupList { get; private set; }
        private bool initialzed = false;
        private string favoritePath = FileSystem.AppDataDirectory + "/favorites.json";
        public FavoritesService(IFileIOService fileIOService)
        {
            this.fileIOService = fileIOService;
        }
        public void InitFavorites()
        {
            if (!initialzed)
            {
                try
                {
                    var favoriesStringList = File.ReadAllText(favoritePath);
                    FavoritesGroupList = new M3UList(favoriesStringList);
                    initialzed = true;
                }
                catch
                {
                    FavoritesGroupList = new M3UList { M3UGroups = new Dictionary<string, M3UGroup>() };
                }
            }
        }
        public void SaveFavoritesListString() => fileIOService.SaveM3U(FavoritesGroupList.GetM3UString(), favoritePath);
        public bool IsChannelInFavorites(M3UChannel channel) =>
            FavoritesGroupList.M3UGroups
            .SelectMany(gl => gl.Value.Channels)
            .Any(c => c.FullChannelString == channel.FullChannelString);
        public void AddChannelToFavory(M3UChannel channel)
        {
            var key = Utils.TrimmedString(channel.Group);
            if (!FavoritesGroupList.M3UGroups.ContainsKey(key))
                FavoritesGroupList.M3UGroups[key] = new M3UGroup(channel.Group, new List<M3UChannel>());
            FavoritesGroupList.M3UGroups[key].AddChannel(channel);
        }
        public void RemoveChannelFromFavorites(M3UChannel channel)
        {
            var key = Utils.TrimmedString(channel.Group);
            FavoritesGroupList.M3UGroups[key].RemoveChannel(channel.FullChannelString);
        }
    }
}
