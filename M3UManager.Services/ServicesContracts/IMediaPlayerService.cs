using M3UManager.Models;

namespace M3UManager.Services.ServicesContracts
{
    public interface IMediaPlayerService
    {
        void PlayStream(string streamUrl);
        void StopStream();
        Task OpenPlayerWindow(string streamUrl, string channelName);
        Task OpenPlayerWindow(M3UChannel channel);
        Task OpenPipPlayer(string streamUrl, string channelName);
        Task OpenPipPlayer(M3UChannel channel);
        void RegisterWindowFactory(Func<string, string, Task> windowFactory);
        void RegisterPipFactory(Func<string, string, Task> pipFactory);
    }
}
