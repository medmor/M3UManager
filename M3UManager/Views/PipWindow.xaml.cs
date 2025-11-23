#nullable enable
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;

namespace M3UManager.Views
{
    public partial class PipWindow : ContentPage
    {
        private bool _isPlaying = true;
        private bool _isSeeking = false;
        private System.Timers.Timer? _controlsTimer;
        private System.Timers.Timer? _progressTimer;

        public event EventHandler? ExpandRequested;
        public event EventHandler? WindowClosed;

        public PipWindow()
        {
            InitializeComponent();

            // Setup controls auto-hide timer
            _controlsTimer = new System.Timers.Timer(3000);
            _controlsTimer.Elapsed += (s, e) => MainThread.BeginInvokeOnMainThread(() =>
            {
                controlsOverlay.IsVisible = false;
            });

            // Setup progress update timer
            _progressTimer = new System.Timers.Timer(500);
            _progressTimer.Elapsed += UpdateProgressUI;
            
            // Cleanup when page is disappearing
            this.Unloaded += (s, e) => Cleanup();
        }

        public void LoadStream(string streamUrl, string channelName)
        {
            channelNameLabel.Text = channelName;
            pipChannelLabel.Text = channelName;
            this.Title = channelName;
            
            mediaElement.Source = MediaSource.FromUri(streamUrl);
            mediaElement.StateChanged += OnMediaStateChanged;
            mediaElement.MediaFailed += OnMediaFailed;
            mediaElement.MediaOpened += OnMediaOpened;
            mediaElement.PositionChanged += OnPositionChanged;

            _progressTimer?.Start();
        }

        private void OnMediaOpened(object? sender, EventArgs e)
        {
            _isPlaying = true;
            UpdatePlayPauseIcon();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (mediaElement.Duration.TotalSeconds > 0)
                {
                    progressSlider.Maximum = mediaElement.Duration.TotalSeconds;
                    durationLabel.Text = FormatTime(mediaElement.Duration);
                }
                else
                {
                    durationLabel.Text = "LIVE";
                    progressSlider.IsEnabled = false;
                }
            });
        }

        private void OnMediaFailed(object? sender, MediaFailedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"PiP media failed: {e.ErrorMessage}");
        }

        private void OnMediaStateChanged(object? sender, MediaStateChangedEventArgs e)
        {
            _isPlaying = e.NewState == MediaElementState.Playing;
            MainThread.BeginInvokeOnMainThread(UpdatePlayPauseIcon);
        }

        private void OnPositionChanged(object? sender, MediaPositionChangedEventArgs e)
        {
            if (!_isSeeking)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    progressSlider.Value = e.Position.TotalSeconds;
                    currentTimeLabel.Text = FormatTime(e.Position);
                });
            }
        }

        private void UpdateProgressUI(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_isSeeking && mediaElement?.CurrentState == MediaElementState.Playing)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (mediaElement.Duration.TotalSeconds > 0)
                    {
                        progressSlider.Value = mediaElement.Position.TotalSeconds;
                        currentTimeLabel.Text = FormatTime(mediaElement.Position);
                    }
                });
            }
        }

        private void OnProgressDragStarted(object? sender, EventArgs e)
        {
            _isSeeking = true;
        }

        private void OnProgressChanged(object? sender, ValueChangedEventArgs e)
        {
            if (_isSeeking)
            {
                currentTimeLabel.Text = FormatTime(TimeSpan.FromSeconds(e.NewValue));
            }
        }

        private void OnProgressDragCompleted(object? sender, EventArgs e)
        {
            _isSeeking = false;
            var newPosition = TimeSpan.FromSeconds(progressSlider.Value);
            mediaElement?.SeekTo(newPosition);
        }

        private void OnPlayPauseClicked(object? sender, EventArgs e)
        {
            if (_isPlaying)
            {
                mediaElement?.Pause();
            }
            else
            {
                mediaElement?.Play();
            }
            ResetControlsTimer();
        }

        private void OnRewind10Clicked(object? sender, EventArgs e)
        {
            if (mediaElement == null) return;
            
            var newPosition = mediaElement.Position - TimeSpan.FromSeconds(10);
            if (newPosition < TimeSpan.Zero) newPosition = TimeSpan.Zero;
            mediaElement.SeekTo(newPosition);
            ResetControlsTimer();
        }

        private void OnForward10Clicked(object? sender, EventArgs e)
        {
            if (mediaElement == null) return;
            
            var newPosition = mediaElement.Position + TimeSpan.FromSeconds(10);
            if (newPosition > mediaElement.Duration) newPosition = mediaElement.Duration;
            mediaElement.SeekTo(newPosition);
            ResetControlsTimer();
        }

        private void OnVolumeClicked(object? sender, EventArgs e)
        {
            if (mediaElement == null || volumeButton == null) return;

            mediaElement.ShouldMute = !mediaElement.ShouldMute;
            volumeButton.Text = mediaElement.ShouldMute ? "🔇" : "🔊";
            ResetControlsTimer();
        }

        private void OnExpandClicked(object? sender, EventArgs e)
        {
            ExpandRequested?.Invoke(this, EventArgs.Empty);
            var window = this.GetParentWindow();
            if (window != null)
            {
                Application.Current?.CloseWindow(window);
            }
        }

        private void OnCloseClicked(object? sender, EventArgs e)
        {
            var window = this.GetParentWindow();
            if (window != null)
            {
                Application.Current?.CloseWindow(window);
            }
        }

        private void OnPlayerTapped(object? sender, TappedEventArgs e)
        {
            controlsOverlay.IsVisible = !controlsOverlay.IsVisible;

            if (controlsOverlay.IsVisible)
            {
                ResetControlsTimer();
            }
            else
            {
                _controlsTimer?.Stop();
            }
        }

        private void OnDragHandlePan(object? sender, PanUpdatedEventArgs e)
        {
            if (e.StatusType == GestureStatus.Running)
            {
                var window = this.GetParentWindow();
                if (window != null)
                {
                    window.X += e.TotalX;
                    window.Y += e.TotalY;
                }
            }
        }

        private void UpdatePlayPauseIcon()
        {
            if (playPauseButton == null) return;

            try
            {
                playPauseButton.Text = _isPlaying ? "⏸" : "▶";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating play/pause icon: {ex.Message}");
            }
        }

        private void ResetControlsTimer()
        {
            _controlsTimer?.Stop();
            _controlsTimer?.Start();
        }

        private string FormatTime(TimeSpan time)
        {
            if (time.TotalHours >= 1)
            {
                return time.ToString(@"hh\:mm\:ss");
            }
            return time.ToString(@"mm\:ss");
        }

        public void Cleanup()
        {
            try
            {
                // Stop and dispose timers
                _controlsTimer?.Stop();
                _controlsTimer?.Dispose();
                _progressTimer?.Stop();
                _progressTimer?.Dispose();
                
                // Stop media playback
                if (mediaElement != null)
                {
                    mediaElement.Stop();
                    mediaElement.Handler?.DisconnectHandler();
                }
                
                WindowClosed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during PipWindow cleanup: {ex.Message}");
            }
        }
    }
}
