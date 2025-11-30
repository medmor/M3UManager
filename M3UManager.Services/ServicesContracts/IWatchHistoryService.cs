using M3UManager.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace M3UManager.Services.ServicesContracts
{
    public interface IWatchHistoryService
    {
        Task AddOrUpdateWatchHistory(M3UChannel channel, TimeSpan position);
        Task<List<WatchHistory>> GetWatchHistory();
        Task<WatchHistory?> GetLastPositionForChannel(string channelUrl);
        Task ClearHistory();
        Task RemoveFromHistory(string channelUrl);
        event EventHandler? HistoryChanged;
    }
}
