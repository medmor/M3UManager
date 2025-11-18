using M3UManager.Models.XtreamModels;
using M3UManager.Services.ServicesContracts;
using System.Text.Json;

namespace M3UManager.Services
{
    public class XtreamService : IXtreamService
    {
        private readonly HttpClient _httpClient;

        public XtreamService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<XtreamUserInfo> GetUserInfoAsync(string serverUrl, string username, string password)
        {
            try
            {
                var url = $"{serverUrl}/player_api.php?username={username}&password={password}";
                var response = await _httpClient.GetStringAsync(url);
                var userInfo = JsonSerializer.Deserialize<XtreamUserInfo>(response);
                return userInfo;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<XtreamCategory>> GetLiveCategoriesAsync(string serverUrl, string username, string password)
        {
            try
            {
                var url = $"{serverUrl}/player_api.php?username={username}&password={password}&action=get_live_categories";
                var response = await _httpClient.GetStringAsync(url);
                var categories = JsonSerializer.Deserialize<List<XtreamCategory>>(response);
                return categories ?? new List<XtreamCategory>();
            }
            catch
            {
                return new List<XtreamCategory>();
            }
        }

        public async Task<List<XtreamChannel>> GetLiveStreamsAsync(string serverUrl, string username, string password)
        {
            try
            {
                var url = $"{serverUrl}/player_api.php?username={username}&password={password}&action=get_live_streams";
                var response = await _httpClient.GetStringAsync(url);
                var streams = JsonSerializer.Deserialize<List<XtreamChannel>>(response);
                return streams ?? new List<XtreamChannel>();
            }
            catch
            {
                return new List<XtreamChannel>();
            }
        }

        public async Task<List<XtreamChannel>> GetLiveStreamsByCategoryAsync(string serverUrl, string username, string password, string categoryId)
        {
            try
            {
                var url = $"{serverUrl}/player_api.php?username={username}&password={password}&action=get_live_streams&category_id={categoryId}";
                var response = await _httpClient.GetStringAsync(url);
                var streams = JsonSerializer.Deserialize<List<XtreamChannel>>(response);
                return streams ?? new List<XtreamChannel>();
            }
            catch
            {
                return new List<XtreamChannel>();
            }
        }

        public async Task<List<XtreamCategory>> GetVodCategoriesAsync(string serverUrl, string username, string password)
        {
            try
            {
                var url = $"{serverUrl}/player_api.php?username={username}&password={password}&action=get_vod_categories";
                var response = await _httpClient.GetStringAsync(url);
                var categories = JsonSerializer.Deserialize<List<XtreamCategory>>(response);
                return categories ?? new List<XtreamCategory>();
            }
            catch
            {
                return new List<XtreamCategory>();
            }
        }

        public async Task<List<XtreamChannel>> GetVodStreamsAsync(string serverUrl, string username, string password)
        {
            try
            {
                var url = $"{serverUrl}/player_api.php?username={username}&password={password}&action=get_vod_streams";
                var response = await _httpClient.GetStringAsync(url);
                var streams = JsonSerializer.Deserialize<List<XtreamChannel>>(response);
                return streams ?? new List<XtreamChannel>();
            }
            catch
            {
                return new List<XtreamChannel>();
            }
        }

        public async Task<List<XtreamCategory>> GetSeriesCategoriesAsync(string serverUrl, string username, string password)
        {
            try
            {
                var url = $"{serverUrl}/player_api.php?username={username}&password={password}&action=get_series_categories";
                var response = await _httpClient.GetStringAsync(url);
                var categories = JsonSerializer.Deserialize<List<XtreamCategory>>(response);
                return categories ?? new List<XtreamCategory>();
            }
            catch
            {
                return new List<XtreamCategory>();
            }
        }

        public async Task<List<XtreamChannel>> GetSeriesAsync(string serverUrl, string username, string password)
        {
            try
            {
                var url = $"{serverUrl}/player_api.php?username={username}&password={password}&action=get_series";
                var response = await _httpClient.GetStringAsync(url);
                var series = JsonSerializer.Deserialize<List<XtreamChannel>>(response);
                return series ?? new List<XtreamChannel>();
            }
            catch
            {
                return new List<XtreamChannel>();
            }
        }

        public string GetStreamUrl(string serverUrl, string username, string password, int streamId, string extension = "m3u8")
        {
            return $"{serverUrl}/live/{username}/{password}/{streamId}.{extension}";
        }

        public string GetVodUrl(string serverUrl, string username, string password, int streamId, string extension = "mp4")
        {
            return $"{serverUrl}/movie/{username}/{password}/{streamId}.{extension}";
        }

        public string GetSeriesUrl(string serverUrl, string username, string password, int seriesId)
        {
            return $"{serverUrl}/series/{username}/{password}/{seriesId}.m3u8";
        }
    }
}
