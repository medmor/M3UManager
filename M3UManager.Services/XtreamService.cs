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
                Console.WriteLine($"[XtreamService] Fetching series list from: {url}");
                
                var response = await _httpClient.GetStringAsync(url);
                Console.WriteLine($"[XtreamService] Series response length: {response?.Length ?? 0} characters");
                
                var series = JsonSerializer.Deserialize<List<XtreamChannel>>(response);
                
                if (series != null && series.Any())
                {
                    Console.WriteLine($"[XtreamService] ? Loaded {series.Count} series");
                    // Log first few series with their IDs
                    foreach (var s in series.Take(3))
                    {
                        Console.WriteLine($"[XtreamService]   - {s.Name}: stream_id={s.StreamId}, series_id={s.SeriesId}, GetId()={s.GetId()}");
                    }
                }
                else
                {
                    Console.WriteLine($"[XtreamService] ?? No series returned from API");
                }
                
                return series ?? new List<XtreamChannel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[XtreamService] ? Error fetching series: {ex.Message}");
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

        public async Task<XtreamSeriesInfo> GetSeriesInfoAsync(string serverUrl, string username, string password, int seriesId)
        {
            try
            {
                var url = $"{serverUrl}/player_api.php?username={username}&password={password}&action=get_series_info&series_id={seriesId}";
                Console.WriteLine($"[XtreamService] Fetching series info from: {url}");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var response = await _httpClient.GetStringAsync(url, cts.Token);
                
                Console.WriteLine($"[XtreamService] Response length: {response?.Length ?? 0} characters");
                Console.WriteLine($"[XtreamService] Response preview: {(response?.Length > 500 ? response.Substring(0, 500) + "..." : response)}");

                if (string.IsNullOrWhiteSpace(response))
                {
                    Console.WriteLine($"[XtreamService] ? Empty response from API");
                    return new XtreamSeriesInfo();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };

                var seriesInfo = JsonSerializer.Deserialize<XtreamSeriesInfo>(response, options);
                
                if (seriesInfo == null)
                {
                    Console.WriteLine($"[XtreamService] ? Failed to deserialize response");
                    return new XtreamSeriesInfo();
                }

                // Ensure collections are initialized (handle nulls from API)
                seriesInfo.Info ??= new SeriesDetails();
                seriesInfo.Episodes ??= new Dictionary<string, List<XtreamEpisode>>();
                seriesInfo.Seasons ??= new List<SeasonInfo>();

                Console.WriteLine($"[XtreamService] ? Successfully deserialized");
                Console.WriteLine($"[XtreamService] Info.Name: {seriesInfo.Info?.Name}");
                Console.WriteLine($"[XtreamService] Seasons count: {seriesInfo.Seasons?.Count ?? 0}");
                Console.WriteLine($"[XtreamService] Episodes count: {seriesInfo.Episodes?.Count ?? 0}");
                
                return seriesInfo;
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"[XtreamService] ?? Request timeout: {ex.Message}");
                return new XtreamSeriesInfo();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[XtreamService] ? HTTP Error: {ex.Message}");
                return new XtreamSeriesInfo();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[XtreamService] ? JSON Deserialization Error: {ex.Message}");
                return new XtreamSeriesInfo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[XtreamService] ? Unexpected Error: {ex.Message}");
                Console.WriteLine($"[XtreamService] Stack trace: {ex.StackTrace}");
                return new XtreamSeriesInfo();
            }
        }

        public string GetEpisodeUrl(string serverUrl, string username, string password, int episodeId, string extension = "mp4")
        {
            return $"{serverUrl}/series/{username}/{password}/{episodeId}.{extension}";
        }
    }
}
