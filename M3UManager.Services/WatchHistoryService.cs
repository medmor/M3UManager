using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace M3UManager.Services
{
    public class WatchHistoryService : IWatchHistoryService
    {
        private List<WatchHistory> _watchHistory = new();
        private readonly string _historyFilePath;
        private const int MaxHistoryItems = 50;

        public event EventHandler? HistoryChanged;

        public WatchHistoryService()
        {
            var appDataPath = FileSystem.AppDataDirectory;
            _historyFilePath = Path.Combine(appDataPath, "watch_history.json");
        }

        public async Task AddOrUpdateWatchHistory(M3UChannel channel, TimeSpan position)
        {
            if (_watchHistory == null)
            {
                await LoadHistory();
            }

            // Find existing entry
            var existing = _watchHistory.FirstOrDefault(h => h.ChannelUrl == channel.Url);
            
            if (existing != null)
            {
                // Update existing entry
                existing.LastWatched = DateTime.Now;
                existing.LastPosition = position;
                existing.ChannelName = channel.Name;
                existing.ChannelLogo = channel.Logo;
            }
            else
            {
                // Add new entry
                var newHistory = new WatchHistory(channel, position);
                _watchHistory.Insert(0, newHistory);
                
                // Keep only the most recent items
                if (_watchHistory.Count > MaxHistoryItems)
                {
                    _watchHistory = _watchHistory.Take(MaxHistoryItems).ToList();
                }
            }

            await SaveHistory();
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<List<WatchHistory>> GetWatchHistory()
        {
            if (_watchHistory == null || _watchHistory.Count == 0)
            {
                await LoadHistory();
            }

            return _watchHistory.OrderByDescending(h => h.LastWatched).ToList();
        }

        public async Task<WatchHistory?> GetLastPositionForChannel(string channelUrl)
        {
            if (_watchHistory == null || _watchHistory.Count == 0)
            {
                await LoadHistory();
            }

            return _watchHistory.FirstOrDefault(h => h.ChannelUrl == channelUrl);
        }

        public async Task ClearHistory()
        {
            _watchHistory.Clear();
            await SaveHistory();
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task RemoveFromHistory(string channelUrl)
        {
            var item = _watchHistory.FirstOrDefault(h => h.ChannelUrl == channelUrl);
            if (item != null)
            {
                _watchHistory.Remove(item);
                await SaveHistory();
                HistoryChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task LoadHistory()
        {
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    var json = await File.ReadAllTextAsync(_historyFilePath);
                    _watchHistory = JsonSerializer.Deserialize<List<WatchHistory>>(json) ?? new List<WatchHistory>();
                }
                else
                {
                    _watchHistory = new List<WatchHistory>();
                }
            }
            catch
            {
                _watchHistory = new List<WatchHistory>();
            }
        }

        private async Task SaveHistory()
        {
            try
            {
                var json = JsonSerializer.Serialize(_watchHistory, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(_historyFilePath, json);
            }
            catch
            {
                // Silently fail
            }
        }
    }
}
