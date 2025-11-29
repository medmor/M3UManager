using M3UManager.Models;
using M3UManager.Models.XtreamModels;
using M3UManager.Services.ServicesContracts;
using M3UManager.UI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace M3UManager.UI.Pages.Editor
{
    public partial class ChannelsList
    {
        [Inject] IJSRuntime JS { get; set; }
        [Inject] IM3UService m3UService { get; set; }
        [Inject] IFavoritesService favoritesService { get; set; }
        [Inject] IXtreamService xtreamService { get; set; }
        [Inject] IMediaPlayerService mediaPlayerService { get; set; }
        [CascadingParameter] public Pages.Editor.Editor editor { get; set; }
        [Parameter] public int M3UListModelId { get; set; }

        public List<M3UChannel>? Channels { get; set; }
        private M3UChannel? selectedChannel;
        private ChannelsDisplay? channelsDisplay;
        private SeriesEpisodesViewer? episodesViewer;
        private bool showEpisodes = false;
        private bool showCategorySelector = false;
        private M3UChannel? selectedChannelForCategories;

        public void OnGroupChanged(List<M3UChannel> channels)
        {
            Channels = channels;
            channelsDisplay?.UpdateChannels(channels);
            StateHasChanged();
        }

        private void OnChannelSelected(M3UChannel channel)
        {
            selectedChannel = channel;
        }

        private async Task ShowEpisodes(M3UChannel series)
        {
            await JS.InvokeVoidAsync("console.log", $"[ChannelsList] ShowEpisodes called for: {series.Name}");
            await JS.InvokeVoidAsync("console.log", $"[ChannelsList] Series Type: {series.Type}");
            await JS.InvokeVoidAsync("console.log", $"[ChannelsList] StreamId: {series.StreamId}");
            await JS.InvokeVoidAsync("console.log", $"[ChannelsList] CategoryId: {series.CategoryId}");
            await JS.InvokeVoidAsync("console.log", $"[ChannelsList] Server URL: {series.XtreamServerUrl}");
            await JS.InvokeVoidAsync("console.log", $"[ChannelsList] Username: {series.XtreamUsername}");
            
            if (series.Type != ContentType.Series)
            {
                await JS.InvokeVoidAsync("console.warn", $"[ChannelsList] Not a series! Type: {series.Type}");
                return;
            }

            if (series.StreamId == 0)
            {
                await JS.InvokeVoidAsync("console.error", "[ChannelsList] StreamId is 0 - cannot fetch episodes!");
                return;
            }

            if (string.IsNullOrEmpty(series.XtreamServerUrl) || 
                string.IsNullOrEmpty(series.XtreamUsername) || 
                string.IsNullOrEmpty(series.XtreamPassword))
            {
                await JS.InvokeVoidAsync("console.error", $"[ChannelsList] Missing Xtream credentials");
                return;
            }

            await JS.InvokeVoidAsync("console.log", "[ChannelsList] All checks passed, loading episodes...");
            
            // Set showEpisodes to true BEFORE loading
            showEpisodes = true;
            StateHasChanged();
            
            if (episodesViewer != null)
            {
                await episodesViewer.LoadSeriesAsync(series, series.XtreamServerUrl, series.XtreamUsername, series.XtreamPassword);
            }
            else
            {
                await JS.InvokeVoidAsync("console.error", "[ChannelsList] episodesViewer is null!");
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
            editor.PlayChannel(channel);
        }

        private async Task PlayChannelPip(M3UChannel channel)
        {
            await mediaPlayerService.OpenPipPlayer(channel.Url, channel.Name);
        }

        private void PlayEpisode(XtreamEpisode episode)
        {
            if (selectedChannel == null || episodesViewer == null)
                return;

            // Build episode URL
            var episodeUrl = xtreamService.GetEpisodeUrl(
                selectedChannel.XtreamServerUrl!,
                selectedChannel.XtreamUsername!,
                selectedChannel.XtreamPassword!,
                int.Parse(episode.Id),
                episode.ContainerExtension);

            // Create a temporary channel for the episode
            var episodeChannel = new M3UChannel
            {
                Name = $"{selectedChannel.Name} - S{episode.Season}E{episode.EpisodeNum} - {episode.Title}",
                Url = episodeUrl,
                Logo = episode.Info.MovieImage,
                Group = selectedChannel.Group,
                Type = ContentType.Series
            };

            editor.PlayChannel(episodeChannel);
        }

        private void ToggleFavorite(M3UChannel channel)
        {
            if (IsChannelInFavorite(channel))
            {
                // Remove from all categories
                var categories = favoritesService.GetCategories();
                foreach (var category in categories)
                {
                    if (favoritesService.IsChannelInCategory(category.Id, channel))
                    {
                        favoritesService.RemoveChannelFromCategory(category.Id, channel);
                    }
                }
                favoritesService.SaveCategories();
                StateHasChanged();
            }
            else
            {
                // Show category selector modal
                selectedChannelForCategories = channel;
                showCategorySelector = true;
                StateHasChanged();
            }
        }

        private async Task OnCategoriesSelected(List<string> categoryIds)
        {
            if (selectedChannelForCategories != null)
            {
                foreach (var categoryId in categoryIds)
                {
                    favoritesService.AddChannelToCategory(categoryId, selectedChannelForCategories);
                }
                favoritesService.SaveCategories();
                StateHasChanged();
            }
        }

        private Task CloseCategorySelector()
        {
            showCategorySelector = false;
            selectedChannelForCategories = null;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private bool IsChannelInFavorite(M3UChannel channel)
        {
            // Check if channel is in any category
            var categories = favoritesService.GetCategories();
            return categories.Any(c => favoritesService.IsChannelInCategory(c.Id, channel));
        }
    }
}
