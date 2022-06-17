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
            var list = groups.Values.SelectMany(d => d.Channels).ToArray();
            var text = string.Join(separator, list.Select(c => c.FullChannelString));
            await File.WriteAllTextAsync(path, text);
        }
        public void OpenWithVlc(string channel)
        {
            if (process != null)
            {
                process.Kill();
            }
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "C:\\Program Files\\VideoLAN\\VLC\\vlc.exe";
            psi.Arguments = "-vvv " + channel.Trim();
            process = Process.Start(psi);
        }
    }
}
