using M3UManager.Services.ServicesContracts;
using System.Diagnostics;

namespace M3UManager.Services
{
    public class MediaPlayerService : IMediaPlayerService
    {
        private Process? currentProcess;

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
    }
}
