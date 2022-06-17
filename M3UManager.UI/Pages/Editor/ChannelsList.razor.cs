using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace M3UManager.UI.Pages.Editor
{
    public partial class ChannelsList
    {
        [Inject] IJSRuntime JS { get; set; }
        [Inject] IFileIOService fileIOService { get; set; }
        [Inject] IFavoritesService favoritesService { get; set; }
        List<M3UChannel>? Channels { get; set; }
        List<M3UChannel> filtredChannels { get; set; } = new List<M3UChannel>();
        int filtredChannelsIndex { get; set; } = 0;
        M3UChannel? selectedChannel { get; set; }
        int channelsToShow = 200;

        public void OnGroupChanged(List<M3UChannel> c)
        {
            Channels = c;
            channelsToShow = c.Count > 200 ? 200 : c.Count - 1;
            StateHasChanged();
        }
        void SelectChannel(M3UChannel c) => selectedChannel = c;
        async Task FilterChannels(ChangeEventArgs args)
        {
            filtredChannels.Clear();
            filtredChannelsIndex = 0;

            string filter = (string)args.Value;

            if (!string.IsNullOrEmpty(filter))
                filtredChannels = Channels!
                    .Take(channelsToShow)
                    .Where(c => c.Name.Contains(filter, System.StringComparison.CurrentCultureIgnoreCase))
                    .ToList();
            if (filtredChannels.Count() > 0)
            {
                await ScrollToFiltred(0);
            }
            await CreateIndicators(filtredChannels.Select(c => c.Name).ToArray());
        }
        async Task ScrollToFiltred(int increment)
        {
            filtredChannelsIndex += increment;
            if (filtredChannelsIndex > filtredChannels.Count() - 1)
                filtredChannelsIndex = 0;
            else if (filtredChannelsIndex < 0)
                filtredChannelsIndex = filtredChannels.Count() - 1;

            await JS.InvokeVoidAsync("ChannelList.scrollToFiltred", filtredChannels[filtredChannelsIndex].Name);
        }
        async Task CreateIndicators(string[] ids) => await JS.InvokeVoidAsync("ChannelList.addIndicators", ids);
        bool FilterButtonDisabled() => filtredChannels.Count() == 0;
        void PlayOnVlc(M3UChannel channel) => fileIOService.OpenWithVlc(channel.Url);
        bool IsChannelInFavorite() => favoritesService.IsChannelInFavorites(selectedChannel);
        void AddToFavorites() => favoritesService.AddChannelToFavory(selectedChannel);
        void RemoveFromFavorites() => favoritesService.RemoveChannelFromFavorites(selectedChannel);
        void LoadMore()
        {
            channelsToShow += 200;
            if (channelsToShow > Channels.Count() - 1)
                channelsToShow = Channels.Count() - 1;
        }

    }
}
