#nullable enable
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;

namespace M3UManager.Views
{
    public partial class PipWindow : ContentPage
    {
        private bool _isPlaying = true;
        private bool _isSeeking = false;
        private System.Timers.Timer? _progressTimer;
        private System.Timers.Timer? _hideControlsTimer;
        private bool _isPointerInside = false;
        private Point _lastPointerPosition = new Point(-1, -1);

        public event EventHandler? ExpandRequested;
        public event EventHandler? WindowClosed;

        public PipWindow()
        {
            InitializeComponent();

            // Initially show controls
            controlsOverlay.IsVisible = true;

            // Setup progress update timer
            _progressTimer = new System.Timers.Timer(500);
            _progressTimer.Elapsed += UpdateProgressUI;
            
            // Setup auto-hide controls timer (5 seconds)
            _hideControlsTimer = new System.Timers.Timer(5000);
            _hideControlsTimer.Elapsed += HideControlsTimerElapsed;
            _hideControlsTimer.AutoReset = false; // Only fire once
            _hideControlsTimer.Start(); // Start the timer on initialization
            
            // Cleanup when page is disappearing
            this.Unloaded += (s, e) => Cleanup();
            
            // Setup media event handlers once
            mediaElement.StateChanged += OnMediaStateChanged;
            mediaElement.MediaFailed += OnMediaFailed;
            mediaElement.MediaOpened += OnMediaOpened;
            mediaElement.PositionChanged += OnPositionChanged;
        }

        public void LoadStream(string streamUrl, string channelName)
        {
            channelNameLabel.Text = channelName;
            pipChannelLabel.Text = channelName;
            this.Title = channelName;
            
            // Stop current stream if playing
            if (mediaElement.CurrentState == MediaElementState.Playing || 
                mediaElement.CurrentState == MediaElementState.Paused)
            {
                mediaElement.Stop();
            }
            
            mediaElement.Source = MediaSource.FromUri(streamUrl);
            
            _progressTimer?.Start();
        }
        
        public void UpdateStream(string streamUrl, string channelName)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadStream(streamUrl, channelName);
            });
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
        }

        private void OnRewind10Clicked(object? sender, EventArgs e)
        {
            if (mediaElement == null) return;
            
            var newPosition = mediaElement.Position - TimeSpan.FromSeconds(10);
            if (newPosition < TimeSpan.Zero) newPosition = TimeSpan.Zero;
            mediaElement.SeekTo(newPosition);
        }

        private void OnForward10Clicked(object? sender, EventArgs e)
        {
            if (mediaElement == null) return;
            
            var newPosition = mediaElement.Position + TimeSpan.FromSeconds(10);
            if (newPosition > mediaElement.Duration) newPosition = mediaElement.Duration;
            mediaElement.SeekTo(newPosition);
        }

        private void OnVolumeClicked(object? sender, EventArgs e)
        {
            if (mediaElement == null || volumeButton == null) return;

            mediaElement.ShouldMute = !mediaElement.ShouldMute;
            volumeButton.Text = mediaElement.ShouldMute ? "🔇" : "🔊";
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

        private void OnPointerEntered(object? sender, PointerEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[PipWindow] PointerEntered - Showing controls");
            _isPointerInside = true;
            _lastPointerPosition = e.GetPosition((View?)sender) ?? new Point(-1, -1);
            ShowControls();
            StartHideControlsTimer();
        }

        private void OnPointerExited(object? sender, PointerEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[PipWindow] PointerExited - Starting hide timer");
            _isPointerInside = false;
            StartHideControlsTimer();
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isPointerInside)
            {
                var currentPosition = e.GetPosition((View?)sender) ?? new Point(-1, -1);
                
                // Check if there's actual movement (threshold of 5 pixels to avoid micro-movements)
                var deltaX = Math.Abs(currentPosition.X - _lastPointerPosition.X);
                var deltaY = Math.Abs(currentPosition.Y - _lastPointerPosition.Y);
                
                if (deltaX > 5 || deltaY > 5)
                {
                    System.Diagnostics.Debug.WriteLine($"[PipWindow] PointerMoved - Actual movement detected (delta: {deltaX:F1}, {deltaY:F1}) - Showing controls and resetting timer");
                    _lastPointerPosition = currentPosition;
                    ShowControls();
                    StartHideControlsTimer();
                }
            }
        }

        private void ShowControls()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (controlsOverlay.Opacity < 1)
                {
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    System.Diagnostics.Debug.WriteLine($"[{timestamp}] [PipWindow] ShowControls - Changing opacity from {controlsOverlay.Opacity} to 1");
                }
                controlsOverlay.Opacity = 1;
            });
        }

        private void StartHideControlsTimer()
        {
            System.Diagnostics.Debug.WriteLine("[PipWindow] StartHideControlsTimer - Timer restarted (5 seconds)");
            _hideControlsTimer?.Stop();
            _hideControlsTimer?.Start();
        }

        private void HideControlsTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] [PipWindow] HideControlsTimerElapsed - _isPointerInside: {_isPointerInside}, Current opacity: {controlsOverlay.Opacity}");
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                controlsOverlay.Opacity = 0;
            });
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
                // Stop and dispose progress timer
                _progressTimer?.Stop();
                _progressTimer?.Dispose();
                
                // Stop and dispose hide controls timer
                _hideControlsTimer?.Stop();
                _hideControlsTimer?.Dispose();
                
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
