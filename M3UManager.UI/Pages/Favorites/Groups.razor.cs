using M3UManager.Models;
using M3UManager.Services.FavoriteServices;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace M3UManager.UI.Pages.Favorites
{
    public partial class Groups
    {
        [Inject] IJSRuntime js { get; set; }
        [Inject] IFavoritesService favoritesService { get; set; }

        // [Parameter] public M3UGroupList M3UListModel { get; set; }
        [Parameter] public string Id { get; set; } = "";
        ChannelsList channelsList;
        Dictionary<string, M3UGroup> filtredGroups;
        string groupFilterString = "";

        protected override void OnInitialized()
        {
            filtredGroups = favoritesService.FavoritesGroupList.M3UGroups;
        }

        void OnSelectGroupsInput(ChangeEventArgs args)
        {
            favoritesService.SelectedGroups = (string[])args.Value;
            List<M3UChannel> channels = new List<M3UChannel>();
            foreach (var key in (string[])args.Value)
            {
                channels = channels.Concat(favoritesService.FavoritesGroupList.M3UGroups[key].Channels).ToList();
            }
            channelsList.OnGroupChanged(channels);
        }
        void FilterGroups(ChangeEventArgs args)
        {
            groupFilterString = (string)args.Value;
            if (string.IsNullOrEmpty(groupFilterString))
                filtredGroups = favoritesService.FavoritesGroupList.M3UGroups;
            else
            {
                filtredGroups = favoritesService.FavoritesGroupList.M3UGroups
                    .Where(g => g.Value.Name.Contains(groupFilterString, System.StringComparison.CurrentCultureIgnoreCase))
                    .ToDictionary(g => g.Key, g => g.Value);
            }
        }
    }
}
