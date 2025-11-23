using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;

namespace M3UManager.UI.Pages.Editor
{
    public partial class VideoPlayer
    {
        [Inject] private IMediaPlayerService MediaPlayerService { get; set; } = default!;

        public async void PlayChannel(M3UChannel channel)
        {
            // Open native media player window (independent window)
            await MediaPlayerService.OpenPlayerWindow(channel.Url, channel.Name);
        }
    }
}
