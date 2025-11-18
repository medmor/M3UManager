using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;

namespace M3UManager.UI.Pages.Favorites
{
    public partial class Favorites
    {
        [Inject] private IFavoritesService favoritesService { get; set; }

        private int GetFavoritesCount()
        {
            if (favoritesService.FavoritesGroupList?.M3UGroups == null)
                return 0;
            return favoritesService.FavoritesGroupList.M3UGroups.Values.Sum(g => g.Channels.Count());
        }
    }
}
