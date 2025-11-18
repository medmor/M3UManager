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

        public void PlayChannel(M3UChannel channel)
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
            
            // Launch or switch stream in external player
            MediaPlayerService.PlayStream(streamUrl);
        }

        public void ClosePlayer()
        {
            isVisible = false;
            streamUrl = string.Empty;
            channelName = string.Empty;
            
            // Stop the media player
            MediaPlayerService.StopStream();
            
            StateHasChanged();
        }
    }
}
