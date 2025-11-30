using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;

namespace M3UManager.UI.Pages.RecentlyWatched
{
    public partial class RecentlyWatched : IDisposable
    {
        [Inject] IWatchHistoryService watchHistoryService { get; set; } = default!;
        [Inject] IMediaPlayerService mediaPlayerService { get; set; } = default!;

        private List<WatchHistory> historyItems = new();
        private bool isLoading = true;
        private bool showClearDialog = false;

        protected override async Task OnInitializedAsync()
        {
            watchHistoryService.HistoryChanged += OnHistoryChanged;
            await LoadHistory();
        }

        private async Task LoadHistory()
        {
            isLoading = true;
            StateHasChanged();

            try
            {
                historyItems = await watchHistoryService.GetWatchHistory();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading history: {ex.Message}");
                historyItems = new List<WatchHistory>();
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async void OnHistoryChanged(object? sender, EventArgs e)
        {
            await InvokeAsync(async () =>
            {
                await LoadHistory();
            });
        }

        private async Task PlayChannel(WatchHistory item)
        {
            var channel = item.ToChannel();
            await mediaPlayerService.OpenPlayerWindow(channel);
        }

        private async Task PlayInPip(WatchHistory item)
        {
            var channel = item.ToChannel();
            await mediaPlayerService.OpenPipPlayer(channel);
        }

        private async Task RemoveItem(WatchHistory item)
        {
            await watchHistoryService.RemoveFromHistory(item.ChannelUrl);
        }

        private void ShowClearConfirmation()
        {
            showClearDialog = true;
            StateHasChanged();
        }

        private void CloseClearDialog()
        {
            showClearDialog = false;
            StateHasChanged();
        }

        private async Task ClearHistory()
        {
            await watchHistoryService.ClearHistory();
            showClearDialog = false;
            StateHasChanged();
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)}w ago";

            return dateTime.ToShortDateString();
        }

        private string FormatTime(TimeSpan time)
        {
            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}:{time.Minutes:D2}:{time.Seconds:D2}";
            else
                return $"{time.Minutes}:{time.Seconds:D2}";
        }

        public void Dispose()
        {
            watchHistoryService.HistoryChanged -= OnHistoryChanged;
        }
    }
}
