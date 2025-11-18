using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace M3UManager.UI.Pages.Editor
{
    public partial class ChannelsList
    {
    [Inject] IJSRuntime JS { get; set; } = default!;
    [Inject] IM3UService m3UService { get; set; } = default!;
    [Inject] IFileIOService fileIOService { get; set; } = default!;
    [Inject] IFavoritesService favoritesService { get; set; } = default!;
        [Parameter] public int M3UListModelId { get; set; }
    [CascadingParameter] public Editor editor { get; set; } = default!;
        List<M3UChannel>? Channels { get; set; }
        List<M3UChannel> filtredChannels { get; set; } = new List<M3UChannel>();
        int filtredChannelsIndex { get; set; } = 0;
    List<M3UChannel> selectedChannels { get; set; } = new();
    M3UChannel? selectedChannel { get; set; }
        int channelsToShow = 200;

        public void OnGroupChanged(List<M3UChannel> c)
        {
            Channels = c;
            channelsToShow = c.Count > 200 ? 200 : c.Count - 1;
            selectedChannel = null;
            selectedChannels = new();
            StateHasChanged();
        }
        public void ToggleSelectChannel(M3UChannel c)
        {
            if (selectedChannels.Contains(c))
                selectedChannels.Remove(c);
            else
                selectedChannels.Add(c);
            selectedChannel = selectedChannels.LastOrDefault();
        }
        async Task FilterChannels(ChangeEventArgs args)
        {
            filtredChannels.Clear();
            filtredChannelsIndex = 0;

            string filter = args.Value?.ToString() ?? string.Empty;

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
        async Task RemoveChannels()
        {
            var cmd = new Services.M3UEditorCommands.DeleteChannelsFromGroupsCommand(m3UService, M3UListModelId, selectedChannels, m3UService.SelectedGroups);
            cmd.Execute();
            editor.Commands.Add(cmd);
            await JS.InvokeVoidAsync("ChannelList.deselectItems");
            if (Channels is not null)
                OnGroupChanged(Channels.Where(c => !selectedChannels.Contains(c)).ToList());
        }
        void PlayChannel() 
        { 
            if (selectedChannel is not null) 
                editor.PlayChannel(selectedChannel); 
        }
        bool IsChannelInFavorite() => selectedChannel is not null && favoritesService.IsChannelInFavorites(selectedChannel);
    void AddToFavorites() { if (selectedChannel is not null) favoritesService.AddChannelToFavory(selectedChannel); }
    void RemoveFromFavorites() { if (selectedChannel is not null) favoritesService.RemoveChannelFromFavorites(selectedChannel); }
        void LoadMore()
        {
            channelsToShow += 200;
            if (Channels is not null && channelsToShow > Channels.Count() - 1)
                channelsToShow = Channels.Count() - 1;
        }
        string OptionClass(M3UChannel channel)
        {
            var cl = "";
            if (filtredChannels.Contains(channel))
            {
                cl += "bg-success";
            }
            if (selectedChannel == channel)
            {
                cl += " border border-3 border-warning";
            }
            return cl;
        }
    }
}
