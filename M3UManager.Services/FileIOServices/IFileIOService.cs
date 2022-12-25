namespace M3UManager.Services.FileIOServices
{
    public interface IFileIOService
    {
        Task<string> OpenM3U();

        Task SaveM3U(string m3uListString, string path = "C:\\Users\\enakr\\Downloads");
        void OpenWithVlc(string channel);
    }
}
