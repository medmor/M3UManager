
using Microsoft.AspNetCore.Components;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.UI.Shared
{
    public partial class NavMenu
    {
        [Inject] private IFileIOService fileService { get; set; }

        async Task OpenFile()
        {
            var txt = await fileService.OpenM3U();
        }

    }
}
