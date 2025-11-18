using M3UManager.Models;
using M3UManager.Models.XtreamModels;
using M3UManager.Services.ServicesContracts;
using M3UManager.UI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace M3UManager.UI.Pages.Favorites
{
    public partial class Favorites
    {
        [Inject] private IFavoritesService favoritesService { get; set; }
        [Inject] private IXtreamService xtreamService { get; set; }
        [Inject] private IJSRuntime JS { get; set; }

        private ChannelsDisplay? channelsDisplay;
        private SeriesEpisodesViewer? episodesViewer;
        private Dictionary<string, M3UGroup> filteredGroups = new();
        private List<M3UChannel> selectedChannels = new();
        private M3UChannel? selectedChannel;
        private string[] selectedGroups = Array.Empty<string>();
        private bool showEpisodes = false;

        protected override void OnInitialized()
        {
            favoritesService.InitFavorites();
            if (favoritesService.FavoritesGroupList?.M3UGroups != null)
            {
                filteredGroups = favoritesService.FavoritesGroupList.M3UGroups;
            }
        }

        private int GetFavoritesCount()
        {
            if (favoritesService.FavoritesGroupList?.M3UGroups == null)
                return 0;
            return favoritesService.FavoritesGroupList.M3UGroups.Values.Sum(g => g.Channels.Count());
        }

        private void OnSelectGroupsInput(ChangeEventArgs args)
        {
            selectedGroups = (string[])args.Value;
            selectedChannels.Clear();
            
            foreach (var key in selectedGroups)
            {
                if (favoritesService.FavoritesGroupList.M3UGroups.TryGetValue(key, out var group))
                {
                    selectedChannels.AddRange(group.Channels);
                }
            }
            
            channelsDisplay?.UpdateChannels(selectedChannels);
            StateHasChanged();
        }

        private void FilterGroups(ChangeEventArgs args)
        {
            var filterText = args.Value?.ToString() ?? string.Empty;
            
            if (string.IsNullOrEmpty(filterText))
            {
                filteredGroups = favoritesService.FavoritesGroupList.M3UGroups;
            }
            else
            {
                filteredGroups = favoritesService.FavoritesGroupList.M3UGroups
                    .Where(g => g.Value.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(g => g.Key, g => g.Value);
            }
            
            StateHasChanged();
        }

        private void OnChannelSelected(M3UChannel channel)
        {
            selectedChannel = channel;
        }

        private async Task ShowEpisodes(M3UChannel series)
        {
            try
            {
                await JS.InvokeVoidAsync("console.log", $"[Favorites] ShowEpisodes called for: {series.Name}");
                await JS.InvokeVoidAsync("console.log", $"[Favorites] Series Type: {series.Type}");
                await JS.InvokeVoidAsync("console.log", $"[Favorites] StreamId: {series.StreamId}");
                await JS.InvokeVoidAsync("console.log", $"[Favorites] Server URL: {series.XtreamServerUrl}");
                await JS.InvokeVoidAsync("console.log", $"[Favorites] Username: {series.XtreamUsername}");
                
                if (series.Type != ContentType.Series)
                {
                    await JS.InvokeVoidAsync("console.warn", $"[Favorites] Not a series! Type: {series.Type}");
                    return;
                }

                if (string.IsNullOrEmpty(series.XtreamServerUrl) || 
                    string.IsNullOrEmpty(series.XtreamUsername) || 
                    string.IsNullOrEmpty(series.XtreamPassword))
                {
                    await JS.InvokeVoidAsync("console.error", "[Favorites] Missing Xtream credentials");
                    return;
                }
                
                // Set showEpisodes to true BEFORE loading
                showEpisodes = true;
                StateHasChanged();
                
                if (episodesViewer != null)
                {
                    await episodesViewer.LoadSeriesAsync(series, series.XtreamServerUrl, series.XtreamUsername, series.XtreamPassword);
                }
                else
                {
                    await JS.InvokeVoidAsync("console.error", "[Favorites] Episodes viewer is null!");
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("console.error", $"[Favorites] Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private Task CloseEpisodes()
        {
            showEpisodes = false;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private void PlayChannel(M3UChannel channel)
        {
            // Navigate to play the channel
            // You might need to inject a video player service here
        }

        private void PlayEpisode(XtreamEpisode episode)
        {
            if (selectedChannel == null || episodesViewer == null)
                return;

            // Build episode URL and play it
            var episodeUrl = xtreamService.GetEpisodeUrl(
                selectedChannel.XtreamServerUrl!,
                selectedChannel.XtreamUsername!,
                selectedChannel.XtreamPassword!,
                int.Parse(episode.Id),
                episode.ContainerExtension);

            // TODO: Trigger video player with episodeUrl
            Console.WriteLine($"Playing episode: {episode.Title} - {episodeUrl}");
        }

        private void RemoveFromFavorites(M3UChannel channel)
        {
            favoritesService.RemoveChannelFromFavorites(channel);
            
            // Refresh the display
            OnSelectGroupsInput(new ChangeEventArgs { Value = selectedGroups });
            favoritesService.SaveFavoritesListString();
            StateHasChanged();
        }

        private string GetDisplayGroupName(string groupName)
        {
            // Remove content type prefixes from display
            var prefixes = new[]
            {
                "📺 Live TV - ",
                "🎬 Movies - ",
                "📺 TV Shows - ",
                "Live TV - ",
                "Movies - ",
                "TV Shows - "
            };

            foreach (var prefix in prefixes)
            {
                if (groupName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return groupName.Substring(prefix.Length);
                }
            }

            return groupName;
        }
    }
}
