using M3UManager.Models;

namespace M3UManager.Services.ServicesContracts
{
    public interface IFileIOService
    {
        Task<string> OpenM3U();

        Task SaveDictionaryAsM3U(Dictionary<string, M3UGroup> groups, string path = "C:\\Users\\enakr\\Downloads");
        Task OpenWithPlayer(string channel, string channelName = "");
        
        // Cache management
        Task SavePlaylistCache(M3UGroupList playlist, string xtreamUrl);
        Task<(M3UGroupList? playlist, string? xtreamUrl, DateTime? cachedDate)> LoadPlaylistCache();
        Task ClearPlaylistCache();
        bool HasCachedPlaylist();
    }
}
