using M3UManager.Services.FavoriteServices;
using Microsoft.AspNetCore.Components;

namespace M3UManager.UI.Pages.Favorites
{
    public partial class Favorites
    {
        [Inject] private IFavoritesService favoritesService { get; set; }
    }
}
