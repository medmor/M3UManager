using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;

namespace M3UManager.UI.Pages.Favorites
{
    public partial class Favorites
    {
        [Inject] private IFavoritesService favoritesService { get; set; }
    }
}
