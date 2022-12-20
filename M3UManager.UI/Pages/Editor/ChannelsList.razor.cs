using M3UManager.Models;
using M3UManager.Models.Commands;
using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace M3UManager.UI.Pages.Editor
{
    public partial class ChannelsList
    {
        [Inject] IJSRuntime JS { get; set; }
        [Inject] IEditorService editorService { get; set; }
        [Inject] ICommandFactory commandFactory { get; set; }
        [Inject] IFileIOService fileIOService { get; set; }
        [Inject] IFavoritesService favoritesService { get; set; }
        [Parameter] public int M3UListModelId { get; set; }
        [CascadingParameter] public Editor editor { get; set; }
        List<M3UChannel>? Channels { get; set; }
        List<M3UChannel> filtredChannels { get; set; } = new List<M3UChannel>();
        int filtredChannelsIndex { get; set; } = 0;
        List<M3UChannel>? selectedChannels { get; set; }
        M3UChannel? selectedChannel { get; set; }
        int channelsToShow = 200;

        public async Task OnGroupChanged(List<M3UChannel> c)
        {
            Channels = c;
            channelsToShow = c.Count > 200 ? 200 : c.Count - 1;
            selectedChannel = null;
            selectedChannels = null;
            await JS.InvokeVoidAsync("ChannelList.deselectItems");
            StateHasChanged();
        }
        void OnSelectchannelsInput(ChangeEventArgs args)
        {
            var selected = (string[])args.Value!;
            selectedChannels = Channels!.Where(c => selected.Any(cc => cc == c.Name)).ToList();
            selectedChannel = selectedChannels[0];
            editorService.SetSelectedChannels(selectedChannels);
        }
        async Task FilterChannels(ChangeEventArgs args)
        {
            filtredChannels.Clear();
            filtredChannelsIndex = 0;

            string filter = (string)args.Value!;

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
            var cmd = commandFactory.GetCommand(CommandName.RemoveChannelsFromGroups);
            await cmd.Execute();
            editor.Commands.Add(cmd);
            await JS.InvokeVoidAsync("ChannelList.deselectItems");
            await OnGroupChanged(Channels!.Where(c => !selectedChannels.Contains(c)).ToList());
        }
        void PlayOnVlc() => fileIOService.OpenWithVlc(selectedChannel!.Url);
        bool IsChannelInFavorite() => favoritesService.IsChannelInFavorites(selectedChannel);
        void AddToFavorites() => favoritesService.AddChannelToFavory(selectedChannel);
        void RemoveFromFavorites() => favoritesService.RemoveChannelFromFavorites(selectedChannel);
        void LoadMore()
        {
            channelsToShow += 200;
            if (channelsToShow > Channels.Count() - 1)
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
