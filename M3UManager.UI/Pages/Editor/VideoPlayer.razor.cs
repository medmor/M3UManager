using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;

namespace M3UManager.UI.Pages.Editor
{
    public partial class VideoPlayer
    {
        [Inject] private IMediaPlayerService MediaPlayerService { get; set; } = default!;
        
        private bool isVisible = false;
        private string streamUrl = string.Empty;
        private string channelName = string.Empty;
        private string contentType = string.Empty;

        public async void PlayChannel(M3UChannel channel)
        {
            streamUrl = channel.Url;
            channelName = channel.Name;
            contentType = channel.Type.ToString();
            
            // If player is already visible, just switch the stream
            // Otherwise, show the player panel
            if (!isVisible)
            {
                isVisible = true;
            }
            
            StateHasChanged();
            
            // Open native media player window
            await MediaPlayerService.OpenPlayerWindow(streamUrl, channelName);
        }

        public void ClosePlayer()
        {
            isVisible = false;
            streamUrl = string.Empty;
            channelName = string.Empty;
            
            // The player window handles its own cleanup
            
            StateHasChanged();
        }
    }
}
