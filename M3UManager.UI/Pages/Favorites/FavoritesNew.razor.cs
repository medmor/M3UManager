using M3UManager.Models;
using M3UManager.Models.XtreamModels;
using M3UManager.Services.ServicesContracts;
using M3UManager.UI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace M3UManager.UI.Pages.Favorites
{
    public partial class FavoritesNew
    {
        [Inject] private IFavoritesService favoritesService { get; set; } = null!;
        [Inject] private IXtreamService xtreamService { get; set; } = null!;
        [Inject] private IMediaPlayerService mediaPlayerService { get; set; } = null!;
        [Inject] private IJSRuntime JS { get; set; } = null!;

        private ChannelsDisplay? channelsDisplay;
        private SeriesEpisodesViewer? episodesViewer;
        private List<FavoriteCategory> categories = new();
        private FavoriteCategory? selectedCategory;
        private string? selectedCategoryId;
        private M3UChannel? selectedChannel;
        private bool showEpisodes = false;
        private bool showCategoryManager = false;
        
        // Context menu state
        private bool showContextMenu = false;
        private M3UChannel? contextMenuChannel;
        private double contextMenuX = 0;
        private double contextMenuY = 0;

        protected override void OnInitialized()
        {
            favoritesService.InitFavorites();
            LoadCategories();
            
            // Select first category by default
            if (categories.Count > 0)
            {
                SelectCategory(categories[0].Id);
            }
        }

        private void LoadCategories()
        {
            categories = favoritesService.GetCategories();
        }

        private void SelectCategory(string categoryId)
        {
            selectedCategoryId = categoryId;
            selectedCategory = favoritesService.GetCategory(categoryId);
            StateHasChanged();
        }

        private int GetTotalFavoritesCount()
        {
            return categories.Sum(c => c.Channels.Count);
        }

        private int GetSelectedCategoryCount()
        {
            return selectedCategory?.Channels.Count ?? 0;
        }

        private void OnChannelSelected(M3UChannel channel)
        {
            selectedChannel = channel;
        }

        private void OpenCategoryManager()
        {
            showCategoryManager = true;
            StateHasChanged();
        }

        private void CloseCategoryManager()
        {
            showCategoryManager = false;
            StateHasChanged();
        }

        private void OnCategoriesChanged()
        {
            LoadCategories();
            
            // Re-select current category or first category
            if (selectedCategoryId != null)
            {
                selectedCategory = favoritesService.GetCategory(selectedCategoryId);
                if (selectedCategory == null && categories.Count > 0)
                {
                    SelectCategory(categories[0].Id);
                }
            }
            
            StateHasChanged();
        }

        private async Task ShowEpisodes(M3UChannel series)
        {
            try
            {
                if (series.Type != ContentType.Series)
                    return;

                if (string.IsNullOrEmpty(series.XtreamServerUrl) || 
                    string.IsNullOrEmpty(series.XtreamUsername) || 
                    string.IsNullOrEmpty(series.XtreamPassword))
                    return;
                
                showEpisodes = true;
                StateHasChanged();
                
                if (episodesViewer != null)
                {
                    await episodesViewer.LoadSeriesAsync(series, series.XtreamServerUrl, series.XtreamUsername, series.XtreamPassword);
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("console.error", $"Error loading episodes: {ex.Message}");
            }
        }

        private Task CloseEpisodes()
        {
            showEpisodes = false;
            StateHasChanged();
            return Task.CompletedTask;
        }

        public void PlayChannel(M3UChannel channel)
        {
            _ = mediaPlayerService.OpenPlayerWindow(channel);
        }

        private async Task PlayChannelPip(M3UChannel channel)
        {
            await mediaPlayerService.OpenPipPlayer(channel.Url, channel.Name);
        }

        private void PlayEpisode(XtreamEpisode episode)
        {
            if (selectedChannel == null)
                return;

            var episodeUrl = xtreamService.GetEpisodeUrl(
                selectedChannel.XtreamServerUrl!,
                selectedChannel.XtreamUsername!,
                selectedChannel.XtreamPassword!,
                int.Parse(episode.Id),
                episode.ContainerExtension);

            _ = mediaPlayerService.OpenPlayerWindow(episodeUrl, $"{selectedChannel.Name} - {episode.Title}");
        }

        private void RemoveFromCategory(M3UChannel channel)
        {
            if (selectedCategoryId == null)
                return;

            favoritesService.RemoveChannelFromCategory(selectedCategoryId, channel);
            
            // Refresh category
            selectedCategory = favoritesService.GetCategory(selectedCategoryId);
            LoadCategories();
            StateHasChanged();
        }

        private void ShowChannelContextMenu(M3UChannel channel)
        {
            contextMenuChannel = channel;
            showContextMenu = true;
            
            // Position near mouse - you could get actual mouse position via JS interop
            contextMenuX = 100;
            contextMenuY = 100;
            
            StateHasChanged();
        }

        private void CloseContextMenu()
        {
            showContextMenu = false;
            contextMenuChannel = null;
            StateHasChanged();
        }

        private void MoveToCategory(string toCategoryId)
        {
            if (contextMenuChannel == null || selectedCategoryId == null)
                return;

            favoritesService.MoveChannelToCategory(contextMenuChannel, selectedCategoryId, toCategoryId);
            
            // Refresh
            selectedCategory = favoritesService.GetCategory(selectedCategoryId);
            LoadCategories();
            CloseContextMenu();
        }

        private void RemoveFromAllCategories()
        {
            if (contextMenuChannel == null)
                return;

            favoritesService.RemoveChannelFromFavorites(contextMenuChannel);
            
            // Refresh
            if (selectedCategoryId != null)
            {
                selectedCategory = favoritesService.GetCategory(selectedCategoryId);
            }
            LoadCategories();
            CloseContextMenu();
        }
    }
}
