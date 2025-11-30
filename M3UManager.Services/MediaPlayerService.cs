using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using System.Diagnostics;

namespace M3UManager.Services
{
    public class MediaPlayerService : IMediaPlayerService
    {
        private Process? currentProcess;
        private Func<string, string, Task>? _windowFactory;
        private Func<string, string, Task>? _pipFactory;
        private readonly List<WeakReference> _playerWindows = new();
        private readonly IWatchHistoryService _watchHistoryService;
        private M3UChannel? _currentChannel;

        public MediaPlayerService(IWatchHistoryService watchHistoryService)
        {
            _watchHistoryService = watchHistoryService;
        }

        public void RegisterWindowFactory(Func<string, string, Task> windowFactory)
        {
            _windowFactory = windowFactory;
        }

        public void RegisterPipFactory(Func<string, string, Task> pipFactory)
        {
            _pipFactory = pipFactory;
        }

        public void PlayStream(string streamUrl)
        {
            try
            {
                // Try to launch VLC
                currentProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "vlc",
                        Arguments = $"\"{streamUrl}\"",
                        UseShellExecute = true
                    }
                };
                currentProcess.Start();
            }
            catch
            {
                // VLC not found, try with default program
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = streamUrl,
                        UseShellExecute = true
                    });
                }
                catch
                {
                    // Could not open stream
                }
            }
        }

        public void StopStream()
        {
            try
            {
                if (currentProcess != null && !currentProcess.HasExited)
                {
                    currentProcess.Kill();
                    currentProcess.Dispose();
                }
            }
            catch { }
            finally
            {
                currentProcess = null;
            }
        }

        public async Task OpenPlayerWindow(string streamUrl, string channelName)
        {
            // Stop any VLC process (but allow multiple native player windows)
            StopStream();

            // Use the registered factory to open a new window
            if (_windowFactory != null)
            {
                await _windowFactory(streamUrl, channelName);
            }
            else
            {
                throw new InvalidOperationException("Window factory not registered. Call RegisterWindowFactory during app initialization.");
            }
        }

        public async Task OpenPlayerWindow(M3UChannel channel)
        {
            _currentChannel = channel;
            // Track with initial position of 0
            await _watchHistoryService.AddOrUpdateWatchHistory(channel, TimeSpan.Zero);
            await OpenPlayerWindow(channel.Url, channel.Name);
        }

        public async Task OpenPipPlayer(string streamUrl, string channelName)
        {
            // Stop any VLC process
            StopStream();

            // Use the registered PiP factory
            if (_pipFactory != null)
            {
                await _pipFactory(streamUrl, channelName);
            }
            else
            {
                throw new InvalidOperationException("PiP factory not registered. Call RegisterPipFactory during app initialization.");
            }
        }

        public async Task OpenPipPlayer(M3UChannel channel)
        {
            _currentChannel = channel;
            // Track with initial position of 0
            await _watchHistoryService.AddOrUpdateWatchHistory(channel, TimeSpan.Zero);
            await OpenPipPlayer(channel.Url, channel.Name);
        }
    }
}
