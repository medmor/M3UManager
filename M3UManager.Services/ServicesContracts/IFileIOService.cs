using M3UManager.Models;

namespace M3UManager.Services.ServicesContracts
{
    public interface IFileIOService
    {
        Task<string> OpenM3U();

        Task SaveDictionaryAsM3U(Dictionary<string, M3UGroup> groups, string path = "C:\\Users\\enakr\\Downloads");
        Task OpenWithVlc(string channel);
    }
}
