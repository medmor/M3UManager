using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using System.Diagnostics;
using System.Text.Json;

namespace M3UManager.Services
{
    public class FileIO : IFileIOService
    {
        private readonly string cacheDirectory;
        private readonly string cacheFilePath;
        private readonly string cacheMetaPath;
        private string separator = "\n";
        
        public FileIO()
        {
            //separator = Environment.OSVersion.Platform == PlatformID.Win32NT ? "\r\n" : "\n";
            
            // Set up cache directory
            cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "M3UManager");
            cacheFilePath = Path.Combine(cacheDirectory, "playlist_cache.json");
            cacheMetaPath = Path.Combine(cacheDirectory, "cache_meta.json");
            
            // Ensure cache directory exists
            if (!Directory.Exists(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
            }
        }
        private Process process;
        public async Task<string> OpenM3U()
        {
            var options = new PickOptions()
            {
                PickerTitle = "Please select a comic file",
                FileTypes = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { ".m3u" } },
                    { DevicePlatform.WinUI, new[] { ".m3u" } },

                })
            };
            var dialog = await FilePicker.Default.PickAsync(options);

            if (dialog != null)
            {
                //OpenedM3UUrl = dialog.FileName;
                return File.ReadAllText(dialog.FullPath);
            }
            return string.Empty;
        }
        public async Task<string> DownLoadM3U()
        {
            var client = new System.Net.Http.HttpClient();
            string requestString = @"http://grl.faststr.com/get.php?username=Talhikha1_397115&password=8gSEUCTI&type=m3u_plus&output=mpegts&token=XNi8bDMqGjYhj9iWLSioB1g0";

            var GetTask = client.GetAsync(requestString);
            GetTask.Wait(1000); // WebCommsTimeout is in milliseconds

            if (!GetTask.Result.IsSuccessStatusCode)
            {
                return string.Empty;
            }
            return await GetTask.Result.Content.ReadAsStringAsync();

        }
        public async Task SaveDictionaryAsM3U(Dictionary<string, M3UGroup> groups, string path = "C:\\Users\\enakr\\Downloads")
        {
            if (!path.Contains(".json")) // for favorite save... to reformate later
                path = Path.Combine(path, "channels.m3u");

            var channels = groups.Values.SelectMany(d => d.Channels);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 1024 * 64, useAsync: true);
            using var writer = new StreamWriter(stream);
            foreach (var c in channels)
            {
                // Maintain original save format used elsewhere: "#EXTINF:" + FullChannelString
                await writer.WriteAsync("#EXTINF:");
                await writer.WriteAsync(c.FullChannelString);
            }
            await writer.FlushAsync();
        }
        public async Task OpenWithVlc(string channel)
        {
            if (process != null)
            {
                process.Kill();
            }

            string vlcPath = string.Empty;

            if (File.Exists("C:\\Program Files\\VideoLAN\\VLC\\vlc.exe"))
            {
                vlcPath = "C:\\Program Files\\VideoLAN\\VLC\\vlc.exe";
            }
            else if (File.Exists("C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe"))
            {
                vlcPath = "C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe";
            }
            else
            {
                var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
                if (page != null)
                {
                    await page.DisplayAlert("Error", "VLC not found", "OK");
                }
                return;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = vlcPath,
                    Arguments = "-vvv " + channel.Trim(),
                    WorkingDirectory = Path.GetDirectoryName(vlcPath)
                };

                process = Process.Start(psi);
            }
            catch (Exception ex)
            {
                var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
                if (page != null)
                {
                    await page.DisplayAlert("Error", $"Failed to open VLC: {ex.Message}", "OK");
                }
            }
        }

        public async Task SavePlaylistCache(M3UGroupList playlist, string xtreamUrl)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                // Save playlist data
                var playlistJson = JsonSerializer.Serialize(playlist, options);
                await File.WriteAllTextAsync(cacheFilePath, playlistJson);
                
                // Save metadata
                var metadata = new
                {
                    XtreamUrl = xtreamUrl,
                    CachedDate = DateTime.Now
                };
                var metaJson = JsonSerializer.Serialize(metadata, options);
                await File.WriteAllTextAsync(cacheMetaPath, metaJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving cache: {ex.Message}");
            }
        }

        public async Task<(M3UGroupList? playlist, string? xtreamUrl, DateTime? cachedDate)> LoadPlaylistCache()
        {
            try
            {
                if (!File.Exists(cacheFilePath) || !File.Exists(cacheMetaPath))
                {
                    return (null, null, null);
                }
                
                // Load playlist data
                var playlistJson = await File.ReadAllTextAsync(cacheFilePath);
                var playlist = JsonSerializer.Deserialize<M3UGroupList>(playlistJson);
                
                // Load metadata
                var metaJson = await File.ReadAllTextAsync(cacheMetaPath);
                var metadata = JsonSerializer.Deserialize<JsonElement>(metaJson);
                
                var xtreamUrl = metadata.GetProperty("XtreamUrl").GetString();
                var cachedDate = metadata.GetProperty("CachedDate").GetDateTime();
                
                return (playlist, xtreamUrl, cachedDate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading cache: {ex.Message}");
                return (null, null, null);
            }
        }

        public Task ClearPlaylistCache()
        {
            try
            {
                if (File.Exists(cacheFilePath))
                {
                    File.Delete(cacheFilePath);
                }
                if (File.Exists(cacheMetaPath))
                {
                    File.Delete(cacheMetaPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing cache: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        public bool HasCachedPlaylist()
        {
            return File.Exists(cacheFilePath) && File.Exists(cacheMetaPath);
        }

    }
}
