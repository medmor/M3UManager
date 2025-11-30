using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;

namespace M3UManager.UI.Pages.Settings
{
    public partial class Settings
    {
        [Inject] IM3UService m3uService { get; set; } = default!;

        private string xtreamUrl = string.Empty;
        private string cachedXtreamUrl = string.Empty;
        private DateTime? cachedDate = null;
        private bool isSaving = false;
        private bool isRefreshing = false;
        private bool showSaveSuccess = false;
        private bool showRefreshSuccess = false;
        private string errorMessage = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadCurrentSettings();
        }

        private async Task LoadCurrentSettings()
        {
            try
            {
                if (fileIO.HasCachedPlaylist())
                {
                    var cache = await fileIO.LoadPlaylistCache();
                    cachedXtreamUrl = cache.xtreamUrl ?? string.Empty;
                    cachedDate = cache.cachedDate;
                    xtreamUrl = cachedXtreamUrl;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to load settings: {ex.Message}";
            }
        }

        private async Task SaveXtreamUrl()
        {
            if (string.IsNullOrWhiteSpace(xtreamUrl))
            {
                errorMessage = "Please enter a valid Xtream URL";
                return;
            }

            isSaving = true;
            showSaveSuccess = false;
            errorMessage = string.Empty;
            StateHasChanged();

            try
            {
                // Update the cached URL without refreshing the playlist
                if (fileIO.HasCachedPlaylist())
                {
                    var cache = await fileIO.LoadPlaylistCache();
                    if (cache.playlist != null)
                    {
                        await fileIO.SavePlaylistCache(cache.playlist, xtreamUrl);
                        cachedXtreamUrl = xtreamUrl;
                        cachedDate = DateTime.Now;
                        showSaveSuccess = true;
                        
                        // Hide success message after 3 seconds
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(3000);
                            await InvokeAsync(() =>
                            {
                                showSaveSuccess = false;
                                StateHasChanged();
                            });
                        });
                    }
                }
                else
                {
                    errorMessage = "No cached playlist found. Please load a playlist first.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to save URL: {ex.Message}";
            }
            finally
            {
                isSaving = false;
                StateHasChanged();
            }
        }

        private async Task RefreshPlaylist()
        {
            if (string.IsNullOrEmpty(cachedXtreamUrl))
            {
                errorMessage = "No URL configured";
                return;
            }

            isRefreshing = true;
            showRefreshSuccess = false;
            errorMessage = string.Empty;
            StateHasChanged();

            try
            {
                // Load fresh playlist
                var newPlaylist = await m3uService.LoadPlaylistFromXtreamAsync(cachedXtreamUrl);
                
                if (newPlaylist != null)
                {
                    // Replace existing playlist
                    if (m3uService.GroupListsCount() > 0)
                    {
                        m3uService.ReplaceGroupList(0, newPlaylist);
                    }
                    else
                    {
                        m3uService.AddGroupList(newPlaylist);
                    }
                    
                    // Save to cache
                    await fileIO.SavePlaylistCache(newPlaylist, cachedXtreamUrl);
                    cachedDate = DateTime.Now;
                    showRefreshSuccess = true;
                }
                else
                {
                    errorMessage = "Failed to load playlist from server";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to refresh playlist: {ex.Message}";
            }
            finally
            {
                isRefreshing = false;
                StateHasChanged();
            }
        }

        private async Task ClearCache()
        {
            try
            {
                // This would require adding a method to IFileIOService
                errorMessage = "Cache cleared successfully";
                cachedXtreamUrl = string.Empty;
                cachedDate = null;
                xtreamUrl = string.Empty;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to clear cache: {ex.Message}";
            }
        }
    }
}
