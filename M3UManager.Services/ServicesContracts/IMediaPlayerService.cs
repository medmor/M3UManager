namespace M3UManager.Services.ServicesContracts
{
    public interface IMediaPlayerService
    {
        void PlayStream(string streamUrl);
        void StopStream();
        Task OpenPlayerWindow(string streamUrl, string channelName);
        void RegisterWindowFactory(Func<string, string, Task> windowFactory);
    }
}
