using M3UManager.Models.XtreamModels;

namespace M3UManager.Services.ServicesContracts
{
    public interface IXtreamService
    {
        Task<XtreamUserInfo> GetUserInfoAsync(string serverUrl, string username, string password);
        
        // Live TV
        Task<List<XtreamCategory>> GetLiveCategoriesAsync(string serverUrl, string username, string password);
        Task<List<XtreamChannel>> GetLiveStreamsAsync(string serverUrl, string username, string password);
        Task<List<XtreamChannel>> GetLiveStreamsByCategoryAsync(string serverUrl, string username, string password, string categoryId);
        
        // Movies (VOD)
        Task<List<XtreamCategory>> GetVodCategoriesAsync(string serverUrl, string username, string password);
        Task<List<XtreamChannel>> GetVodStreamsAsync(string serverUrl, string username, string password);
        
        // TV Shows (Series)
        Task<List<XtreamCategory>> GetSeriesCategoriesAsync(string serverUrl, string username, string password);
        Task<List<XtreamChannel>> GetSeriesAsync(string serverUrl, string username, string password);
        
        string GetStreamUrl(string serverUrl, string username, string password, int streamId, string extension = "m3u8");
        string GetVodUrl(string serverUrl, string username, string password, int streamId, string extension = "mp4");
        string GetSeriesUrl(string serverUrl, string username, string password, int seriesId);
    }
}
