using M3UManager.Services.ServicesContracts;
using M3UManager.Models;
using Microsoft.AspNetCore.Components;

namespace M3UManager.UI.Pages.Editor
{
    public partial class Editor
    {

        [Inject] IM3UService m3uService { get; set; } = default!;
        [Inject] IFileIOService fileIO { get; set; } = default!;
        
        [Parameter] public string ContentTypeFilter { get; set; } = string.Empty;
        [Parameter] public string PageTitle { get; set; } = "Playlist Manager";
        [Parameter] public string PageIcon { get; set; } = "bi bi-collection-play";
        
        private bool showXtreamDialog = false;
        private string xtreamUrl = string.Empty;
        private string cachedXtreamUrl = string.Empty;
        private DateTime? cachedDate = null;
        private bool isLoading = false;
        private bool isAutoLoading = false;
        private string errorMessage = string.Empty;
        private VideoPlayer videoPlayer = default!;

        // Static flag to track if cache has been checked once across all Editor instances
        private static bool cacheChecked = false;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            
            // Only check cache on first load (not on tab switches)
            if (!cacheChecked)
            {
                cacheChecked = true;
                
                // Check if there's a cached playlist
                if (fileIO.HasCachedPlaylist())
                {
                    try
                    {
                        isAutoLoading = true;
                        StateHasChanged();
                        
                        var cache = await fileIO.LoadPlaylistCache();
                        if (cache.playlist != null && cache.playlist.M3UGroups != null && cache.playlist.M3UGroups.Count > 0)
                        {
                            cachedXtreamUrl = cache.xtreamUrl ?? "Unknown";
                            cachedDate = cache.cachedDate;
                            
                            // Auto-load cached playlist immediately
                            var currentCount = m3uService.GroupListsCount();
                            m3uService.AddGroupList(cache.playlist);
                            
                            isAutoLoading = false;
                            StateHasChanged();
                            
                            // Refresh in background if URL is available
                            if (!string.IsNullOrEmpty(cachedXtreamUrl) && cachedXtreamUrl != "Unknown")
                            {
                                _ = Task.Run(async () => await RefreshPlaylistInBackground(currentCount, cachedXtreamUrl));
                            }
                        }
                        else
                        {
                            isAutoLoading = false;
                            StateHasChanged();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Silently handle cache loading errors
                        isAutoLoading = false;
                        StateHasChanged();
                    }
                }
            }
        }
        
        private async Task RefreshPlaylistInBackground(int modelIndex, string url)
        {
            try
            {
                // Wait a bit to let the UI render first
                await Task.Delay(1000);
                
                // Download fresh playlist to a temporary location
                var tempPlaylist = await m3uService.LoadPlaylistFromXtreamAsync(url);
                
                if (tempPlaylist != null)
                {
                    // Replace the existing playlist at index 0
                    m3uService.ReplaceGroupList(0, tempPlaylist);
                    
                    // Update cache with the new playlist
                    await fileIO.SavePlaylistCache(tempPlaylist, url);
                    
                    // Update UI
                    await InvokeAsync(() => StateHasChanged());
                }
            }
            catch (Exception ex)
            {
                // Silently fail - user is already using cached version
                System.Diagnostics.Debug.WriteLine($"Background refresh failed: {ex.Message}");
            }
        }
        
        public void Refresh() => StateHasChanged();
        
        async Task OpenFile()
        {
            var textFile = await fileIO.OpenM3U();
            if (!string.IsNullOrEmpty(textFile))
            {
                // Only load if we don't have a playlist yet
                if (m3uService.GroupListsCount() == 0)
                {
                    m3uService.AddGroupList(textFile);
                }
            }
        }

        void ShowXtreamDialog()
        {
            showXtreamDialog = true;
            xtreamUrl = string.Empty;
            errorMessage = string.Empty;
            StateHasChanged();
        }

        void CloseXtreamDialog()
        {
            showXtreamDialog = false;
            xtreamUrl = string.Empty;
            errorMessage = string.Empty;
            isLoading = false;
            StateHasChanged();
        }

        async Task LoadFromXtream()
        {
            if (string.IsNullOrWhiteSpace(xtreamUrl))
                return;

            isLoading = true;
            errorMessage = string.Empty;
            StateHasChanged();

            try
            {
                var currentCount = m3uService.GroupListsCount();
                await m3uService.AddGroupListFromXtreamAsync(xtreamUrl);
                
                // Save to cache
                var playlist = m3uService.GetGroupList(currentCount);
                if (playlist != null)
                {
                    await fileIO.SavePlaylistCache(playlist, xtreamUrl);
                }
                
                CloseXtreamDialog();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to load from Xtream: {ex.Message}";
                isLoading = false;
                StateHasChanged();
            }
        }

        
        public void PlayChannel(M3UChannel channel)
        {
            videoPlayer?.PlayChannel(channel);
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            
            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} week{((int)(timeSpan.TotalDays / 7) != 1 ? "s" : "")} ago";
            
            return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) != 1 ? "s" : "")} ago";
        }

        private int GetTotalChannelCount()
        {
            int total = 0;
            for (int i = 0; i < m3uService.GroupListsCount(); i++)
            {
                var groupList = m3uService.GetGroupList(i);
                if (groupList?.M3UGroups != null)
                {
                    total += groupList.M3UGroups.Values.Sum(g => g.Channels.Count());
                }
            }
            return total;
        }

        private int GetFilteredChannelCount()
        {
            if (string.IsNullOrEmpty(ContentTypeFilter))
                return GetTotalChannelCount();

            int total = 0;
            for (int i = 0; i < m3uService.GroupListsCount(); i++)
            {
                var groupList = m3uService.GetGroupList(i);
                if (groupList?.M3UGroups != null)
                {
                    total += groupList.M3UGroups.Values
                        .Where(g => IsGroupMatchingFilter(g))
                        .Sum(g => g.Channels.Count(c => IsChannelMatchingType(c)));
                }
            }
            return total;
        }

        private bool IsGroupMatchingFilter(M3UGroup group)
        {
            if (string.IsNullOrEmpty(ContentTypeFilter))
                return true;

            // Check if group name starts with the content type prefix
            return group.Name.StartsWith(GetContentTypePrefix(), StringComparison.OrdinalIgnoreCase);
        }

        private bool IsChannelMatchingType(M3UChannel channel)
        {
            if (string.IsNullOrEmpty(ContentTypeFilter))
                return true;

            return ContentTypeFilter switch
            {
                "Movie" => channel.Type == ContentType.Movie,
                "TV Show" => channel.Type == ContentType.Series,
                "Live TV" => channel.Type == ContentType.LiveTV,
                _ => true
            };
        }

        private string GetContentTypePrefix()
        {
            return ContentTypeFilter switch
            {
                "Movie" => "🎬 Movies",
                "TV Show" => "📺 TV Shows",
                "Live TV" => "📺 Live TV",
                _ => ""
            };
        }
    }
}
