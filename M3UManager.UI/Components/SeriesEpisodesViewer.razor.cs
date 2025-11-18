using M3UManager.Models;
using M3UManager.Models.XtreamModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace M3UManager.UI.Components
{
    public partial class SeriesEpisodesViewer
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        
        [Parameter] public bool IsVisible { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback<XtreamEpisode> OnEpisodeSelected { get; set; }
        [Parameter] public bool UseImageProxy { get; set; } = true;

        private bool IsLoading { get; set; }
        private XtreamSeriesInfo? SeriesInfo { get; set; }
        private int SelectedSeason { get; set; } = 1;
        private List<XtreamEpisode> SelectedSeasonEpisodes => GetEpisodesForSeason(SelectedSeason);
        private string? ServerUrl { get; set; }
        private string? Username { get; set; }
        private string? Password { get; set; }
        private string? ErrorMessage { get; set; }
        private string? DebugInfo { get; set; }

        public async Task LoadSeriesAsync(M3UChannel series, string serverUrl, string username, string password)
        {
            await LogDebug($"?? LoadSeriesAsync called for: {series.Name}");
            await LogDebug($"?? Server: {serverUrl}");
            await LogDebug($"?? Username: {username}");
            await LogDebug($"?? StreamId: {series.StreamId}");
            
            IsVisible = true;
            IsLoading = true;
            ErrorMessage = null;
            ServerUrl = serverUrl;
            Username = username;
            Password = password;
            StateHasChanged();

            try
            {
                await LogDebug("?? Fetching series info from Xtream API...");
                
                // Create a timeout task
                var seriesInfoTask = XtreamService.GetSeriesInfoAsync(serverUrl, username, password, series.StreamId);
                var timeoutTask = Task.Delay(35000); // 35 seconds (slightly more than HTTP timeout)
                
                var completedTask = await Task.WhenAny(seriesInfoTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    ErrorMessage = "Request timed out after 35 seconds. The server might be slow or unreachable.";
                    await LogDebug("?? Request timed out!");
                    return;
                }
                
                SeriesInfo = await seriesInfoTask;
                
                if (SeriesInfo == null)
                {
                    ErrorMessage = "Failed to load series information. SeriesInfo is null.";
                    await LogDebug("? SeriesInfo is null!");
                    return;
                }

                await LogDebug($"? Series info loaded: {SeriesInfo.Info?.Name ?? "Unknown"}");
                await LogDebug($"?? Seasons count: {SeriesInfo.Seasons?.Count ?? 0}");
                await LogDebug($"?? Episodes dictionary count: {SeriesInfo.Episodes?.Count ?? 0}");

                // Debug: Show all episode data
                if (SeriesInfo.Episodes != null && SeriesInfo.Episodes.Any())
                {
                    await LogDebug("?? Episodes data structure:");
                    foreach (var kvp in SeriesInfo.Episodes)
                    {
                        await LogDebug($"  Key: '{kvp.Key}' ? {kvp.Value?.Count ?? 0} episodes");
                        if (kvp.Value != null && kvp.Value.Any())
                        {
                            var firstEp = kvp.Value.First();
                            await LogDebug($"    First episode: S{firstEp.Season}E{firstEp.EpisodeNum} - {firstEp.Title}");
                        }
                    }
                }
                else
                {
                    await LogDebug("?? Episodes dictionary is null or empty!");
                }

                // Check if we have seasons
                if (SeriesInfo.Seasons == null || !SeriesInfo.Seasons.Any())
                {
                    await LogDebug("?? No seasons in Seasons list, attempting to build from Episodes dictionary...");
                    
                    // Try to build seasons list from episodes dictionary
                    if (SeriesInfo.Episodes != null && SeriesInfo.Episodes.Any())
                    {
                        SeriesInfo.Seasons = new List<SeasonInfo>();
                        foreach (var kvp in SeriesInfo.Episodes.OrderBy(e => int.TryParse(e.Key, out var n) ? n : 0))
                        {
                            if (int.TryParse(kvp.Key, out var seasonNum))
                            {
                                SeriesInfo.Seasons.Add(new SeasonInfo
                                {
                                    SeasonNumber = seasonNum,
                                    Name = $"Season {seasonNum}",
                                    EpisodeCount = kvp.Value?.Count ?? 0
                                });
                            }
                        }
                        await LogDebug($"? Built {SeriesInfo.Seasons.Count} seasons from episodes dictionary");
                    }
                    else
                    {
                        ErrorMessage = "This series has no seasons or episodes available.";
                        await LogDebug("? Cannot build seasons - Episodes dictionary is empty");
                        return;
                    }
                }

                // Select first season by default
                if (SeriesInfo.Seasons.Any())
                {
                    SelectedSeason = SeriesInfo.Seasons.OrderBy(s => s.SeasonNumber).First().SeasonNumber;
                    await LogDebug($"?? Selected season: {SelectedSeason}");
                    
                    var episodes = GetEpisodesForSeason(SelectedSeason);
                    await LogDebug($"?? Episodes in selected season: {episodes.Count}");
                    await LogDebug($"? Successfully loaded {SeriesInfo.Seasons.Count} season(s) with {SeriesInfo.Episodes.Values.Sum(e => e.Count)} total episodes");
                    
                    // Don't call StateHasChanged here - let finally block handle it
                    await LogDebug($"?? State before finally: IsVisible={IsVisible}, IsLoading={IsLoading}, SeriesInfo!=null={SeriesInfo != null}, Seasons.Any={SeriesInfo.Seasons.Any()}");
                }
                else
                {
                    ErrorMessage = "No seasons found for this series.";
                    await LogDebug("?? Seasons list is still empty after all attempts");
                }

                // Debug: Show raw JSON
                DebugInfo = System.Text.Json.JsonSerializer.Serialize(SeriesInfo, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                await LogDebug($"?? Full JSON response saved to debug panel");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading series: {ex.Message}";
                await LogDebug($"? Exception: {ex.Message}");
                await LogDebug($"?? Stack trace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
                await LogDebug($"?? Finally block - IsVisible={IsVisible}, IsLoading={IsLoading}, SeriesInfo!=null={SeriesInfo != null}, HasSeasons={SeriesInfo?.Seasons?.Any() ?? false}");
                StateHasChanged();
                await LogDebug($"? StateHasChanged called - modal should now be visible");
            }
        }

        private void SelectSeason(int seasonNumber)
        {
            _ = LogDebug($"?? Season selected: {seasonNumber}");
            SelectedSeason = seasonNumber;
            var episodes = GetEpisodesForSeason(seasonNumber);
            _ = LogDebug($"?? Episodes in season {seasonNumber}: {episodes.Count}");
            StateHasChanged();
        }

        private List<XtreamEpisode> GetEpisodesForSeason(int seasonNumber)
        {
            if (SeriesInfo?.Episodes == null)
            {
                _ = LogDebug($"?? SeriesInfo.Episodes is null");
                return new List<XtreamEpisode>();
            }

            var seasonKey = seasonNumber.ToString();
            _ = LogDebug($"?? Looking for season key: '{seasonKey}'");
            _ = LogDebug($"?? Available keys: {string.Join(", ", SeriesInfo.Episodes.Keys)}");
            
            if (SeriesInfo.Episodes.TryGetValue(seasonKey, out var episodes))
            {
                _ = LogDebug($"? Found {episodes?.Count ?? 0} episodes for season {seasonNumber}");
                return episodes ?? new List<XtreamEpisode>();
            }

            _ = LogDebug($"? No episodes found for season key '{seasonKey}'");
            return new List<XtreamEpisode>();
        }

        private async Task SelectEpisode(XtreamEpisode episode)
        {
            await LogDebug($"?? Episode selected: S{episode.Season}E{episode.EpisodeNum} - {episode.Title}");
            await OnEpisodeSelected.InvokeAsync(episode);
        }

        private async Task Close()
        {
            await LogDebug("?? Closing episodes viewer - Close() method called");
            await LogDebug($"?? Call stack trace");
            await JS.InvokeVoidAsync("console.trace", "Close() called from:");
            
            IsVisible = false;
            SeriesInfo = null;
            ErrorMessage = null;
            DebugInfo = null;
            await OnClose.InvokeAsync();
            StateHasChanged();
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        private string GetProxiedImageUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) || !UseImageProxy)
                return imageUrl;

            try
            {
                if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
                    return imageUrl;

                if (uri.Scheme == "https")
                    return imageUrl;

                if (uri.Scheme == "http")
                {
                    var safeDomains = new[] { "pbs.twimg.com", "i.imgur.com", "upload.wikimedia.org", "image.tmdb.org" };
                    if (safeDomains.Any(d => uri.Host.Contains(d, StringComparison.OrdinalIgnoreCase)))
                    {
                        return $"https://{uri.Host}{uri.PathAndQuery}";
                    }

                    var escapedUrl = Uri.EscapeDataString(imageUrl);
                    return $"https://images.weserv.nl/?url={escapedUrl}&w=600&h=400&fit=cover&default=1";
                }

                return imageUrl;
            }
            catch
            {
                return imageUrl;
            }
        }

        private async Task LogDebug(string message)
        {
            Console.WriteLine($"[SeriesEpisodesViewer] {message}");
            try
            {
                await JS.InvokeVoidAsync("console.log", $"[SeriesEpisodesViewer] {message}");
            }
            catch
            {
                // Ignore if JS is not available
            }
        }
    }
}
