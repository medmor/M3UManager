using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using System.Diagnostics;

namespace M3UManager.Services
{
    public class FileIO : IFileIOService
    {
        private readonly string separator = "#EXTINF:";
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

    }
}
