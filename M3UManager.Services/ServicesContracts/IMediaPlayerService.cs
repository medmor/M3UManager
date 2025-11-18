namespace M3UManager.Services.ServicesContracts
{
    public interface IMediaPlayerService
    {
        void PlayStream(string streamUrl);
        void StopStream();
    }
}
