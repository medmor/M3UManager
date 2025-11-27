using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Views
{
    public partial class PlayerWindow : ContentPage
    {
        private string _streamUrl;
        private string _channelName;
        private bool _isPlaying = true;
        private bool _isSeeking = false;
        private System.Timers.Timer? _progressTimer;
        private System.Timers.Timer? _hideControlsTimer;
        private bool _isPointerInside = false;
        private Point _lastPointerPosition = new Point(-1, -1);

        public PlayerWindow(string streamUrl, string channelName)
        {
            InitializeComponent();
            
            _streamUrl = streamUrl;
            _channelName = channelName;
            
            LoadStream(streamUrl, channelName);
            
            // Handle media events
            mediaElement.StateChanged += OnMediaStateChanged;
            mediaElement.MediaFailed += OnMediaFailed;
            mediaElement.MediaOpened += OnMediaOpened;
            mediaElement.PositionChanged += OnPositionChanged;

            // Setup progress update timer
            _progressTimer = new System.Timers.Timer(500); // Update every 500ms
            _progressTimer.Elapsed += UpdateProgressUI;
            _progressTimer.Start();

            // Setup controls auto-hide timer (5 seconds)
            _hideControlsTimer = new System.Timers.Timer(5000);
            _hideControlsTimer.Elapsed += HideControlsTimerElapsed;
            _hideControlsTimer.AutoReset = false;
            _hideControlsTimer.Start();

            // Add keyboard event handler
            this.Loaded += OnPageLoaded;
        }
        
        private void LoadStream(string streamUrl, string channelName)
        {
            _streamUrl = streamUrl;
            _channelName = channelName;
            
            // Stop current stream if playing
            if (mediaElement.CurrentState != MediaElementState.None)
            {
                mediaElement.Stop();
            }
            
            // Load new stream
            mediaElement.Source = MediaSource.FromUri(streamUrl);
            channelNameLabel.Text = channelName;
            mediaElement.Play();
        }
        
        public void UpdateStream(string streamUrl, string channelName)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadStream(streamUrl, channelName);
            });
        }

        private void OnPageLoaded(object? sender, EventArgs e)
        {
            // Register keyboard shortcuts
            var window = this.GetParentWindow();
            if (window != null)
            {
                // Note: MAUI doesn't have built-in keyboard event handling yet
                // We'll use platform-specific code in a future update
            }
        }

        private void OnMediaOpened(object? sender, EventArgs e)
        {
            Console.WriteLine("Media opened successfully");
            _isPlaying = true;
            UpdatePlayPauseButton();
            
            // Update duration
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
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Error", $"Failed to load media: {e.ErrorMessage}", "OK");
            });
        }

        private void OnMediaStateChanged(object? sender, MediaStateChangedEventArgs e)
        {
            Console.WriteLine($"Media state changed: {e.NewState}");
            
            _isPlaying = e.NewState == MediaElementState.Playing;
            MainThread.BeginInvokeOnMainThread(UpdatePlayPauseButton);
            
            if (e.NewState == MediaElementState.Failed)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Playback Error", "The stream failed to play. Please check the stream URL.", "OK");
                });
            }
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
            if (!_isSeeking && mediaElement.CurrentState == MediaElementState.Playing)
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
            // Update time label while dragging
            if (_isSeeking)
            {
                currentTimeLabel.Text = FormatTime(TimeSpan.FromSeconds(e.NewValue));
            }
        }

        private void OnProgressDragCompleted(object? sender, EventArgs e)
        {
            _isSeeking = false;
            var newPosition = TimeSpan.FromSeconds(progressSlider.Value);
            mediaElement.SeekTo(newPosition);
        }

        private void OnPlayPauseClicked(object? sender, EventArgs e)
        {
            if (_isPlaying)
            {
                mediaElement.Pause();
            }
            else
            {
                mediaElement.Play();
            }
            ResetControlsTimer();
        }

        private void OnRewind10Clicked(object? sender, EventArgs e)
        {
            var newPosition = mediaElement.Position - TimeSpan.FromSeconds(10);
            if (newPosition < TimeSpan.Zero) newPosition = TimeSpan.Zero;
            mediaElement.SeekTo(newPosition);
            ResetControlsTimer();
        }

        private void OnForward10Clicked(object? sender, EventArgs e)
        {
            var newPosition = mediaElement.Position + TimeSpan.FromSeconds(10);
            if (newPosition > mediaElement.Duration) newPosition = mediaElement.Duration;
            mediaElement.SeekTo(newPosition);
            ResetControlsTimer();
        }

        private void OnVolumeChanged(object? sender, ValueChangedEventArgs e)
        {
            if (mediaElement == null || volumeButton == null)
                return;
                
            mediaElement.Volume = e.NewValue / 100.0;
            
            try
            {
                // Update volume icon with emoji
                if (e.NewValue == 0)
                {
                    volumeButton.Text = "🔇";
                }
                else if (e.NewValue < 33)
                {
                    volumeButton.Text = "🔈";
                }
                else if (e.NewValue < 66)
                {
                    volumeButton.Text = "🔉";
                }
                else
                {
                    volumeButton.Text = "🔊";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating volume button: {ex.Message}");
            }
        }

        private void OnVolumeToggleClicked(object? sender, EventArgs e)
        {
            if (mediaElement == null || volumeButton == null)
                return;
                
            mediaElement.ShouldMute = !mediaElement.ShouldMute;
            
            try
            {
                if (mediaElement.ShouldMute)
                {
                    volumeButton.Text = "🔇";
                }
                else
                {
                    if (volumeSlider.Value == 0)
                        volumeButton.Text = "🔇";
                    else if (volumeSlider.Value < 33)
                        volumeButton.Text = "🔈";
                    else if (volumeSlider.Value < 66)
                        volumeButton.Text = "🔉";
                    else
                        volumeButton.Text = "🔊";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating volume button: {ex.Message}");
            }

            ResetControlsTimer();
        }

        private void OnFullscreenClicked(object? sender, EventArgs e)
        {
            // Toggle fullscreen mode
            var window = this.GetParentWindow();
            if (window != null)
            {
                // MAUI doesn't have built-in fullscreen support yet
                // This would need platform-specific implementation
                DisplayAlert("Info", "Fullscreen mode will be available in a future update.", "OK");
            }
            ResetControlsTimer();
        }

        private void OnSettingsClicked(object? sender, EventArgs e)
        {
            // Show quality/settings menu
            DisplayAlert("Settings", "Quality selection and settings will be available in a future update.", "OK");
            ResetControlsTimer();
        }

        private void OnPointerEntered(object? sender, PointerEventArgs e)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] [PlayerWindow] PointerEntered - _isPointerInside was: {_isPointerInside}, Opacity: {controlsOverlay.Opacity}");
            _isPointerInside = true;
            _lastPointerPosition = e.GetPosition((View?)sender) ?? new Point(-1, -1);
            ShowControls();
            StartHideControlsTimer();
        }

        private void OnPointerExited(object? sender, PointerEventArgs e)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] [PlayerWindow] PointerExited - _isPointerInside was: {_isPointerInside}, Opacity: {controlsOverlay.Opacity}");
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
                    System.Diagnostics.Debug.WriteLine($"[PlayerWindow] PointerMoved - Actual movement detected (delta: {deltaX:F1}, {deltaY:F1}) - Showing controls and resetting timer");
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
                    System.Diagnostics.Debug.WriteLine($"[{timestamp}] [PlayerWindow] ShowControls - Changing opacity from {controlsOverlay.Opacity} to 1");
                }
                controlsOverlay.Opacity = 1;
            });
        }

        private void StartHideControlsTimer()
        {
            System.Diagnostics.Debug.WriteLine("[PlayerWindow] StartHideControlsTimer - Timer restarted (5 seconds)");
            _hideControlsTimer?.Stop();
            _hideControlsTimer?.Start();
        }

        private void HideControlsTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] [PlayerWindow] HideControlsTimerElapsed - _isPointerInside: {_isPointerInside}, Current opacity: {controlsOverlay.Opacity}");
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                controlsOverlay.Opacity = 0;
            });
        }

        private void UpdatePlayPauseButton()
        {
            if (playPauseButton == null)
                return;
                
            try
            {
                playPauseButton.Text = _isPlaying ? "⏸" : "▶";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating play/pause button: {ex.Message}");
            }
        }

        private void ResetControlsTimer()
        {
            StartHideControlsTimer();
        }

        private string FormatTime(TimeSpan time)
        {
            if (time.TotalHours >= 1)
            {
                return time.ToString(@"hh\:mm\:ss");
            }
            return time.ToString(@"mm\:ss");
        }

        private async void OnPipModeClicked(object sender, EventArgs e)
        {
            var mediaPlayerService = Application.Current?.Handler?.MauiContext?.Services
                .GetService(typeof(IMediaPlayerService)) as IMediaPlayerService;

            if (mediaPlayerService != null)
            {
                OnCloseClicked(sender, e);
                await mediaPlayerService.OpenPipPlayer(_streamUrl, _channelName);
            }
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            // Cleanup timers
            _progressTimer?.Stop();
            _progressTimer?.Dispose();
            _hideControlsTimer?.Stop();
            _hideControlsTimer?.Dispose();

            // Stop and dispose media
            mediaElement.Stop();
            mediaElement.Handler?.DisconnectHandler();
            
            // Close the window
            var window = this.GetParentWindow();
            if (window != null)
            {
                Application.Current?.CloseWindow(window);
            }
        }

        public void Cleanup()
        {
            // Stop and cleanup timers
            _progressTimer?.Stop();
            _progressTimer?.Dispose();
            _hideControlsTimer?.Stop();
            _hideControlsTimer?.Dispose();
            
            // Stop media playback
            mediaElement.Stop();
            mediaElement.Handler?.DisconnectHandler();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Cleanup();
        }

        // Keyboard shortcut handlers (to be implemented with platform-specific code)
        public void HandleKeyPress(string key)
        {
            switch (key.ToLower())
            {
                case "space":
                    OnPlayPauseClicked(this, EventArgs.Empty);
                    break;
                case "arrowleft":
                    OnRewind10Clicked(this, EventArgs.Empty);
                    break;
                case "arrowright":
                    OnForward10Clicked(this, EventArgs.Empty);
                    break;
                case "arrowup":
                    volumeSlider.Value = Math.Min(100, volumeSlider.Value + 5);
                    break;
                case "arrowdown":
                    volumeSlider.Value = Math.Max(0, volumeSlider.Value - 5);
                    break;
                case "f":
                    OnFullscreenClicked(this, EventArgs.Empty);
                    break;
                case "m":
                    OnVolumeToggleClicked(this, EventArgs.Empty);
                    break;
                case "escape":
                    if (controlsOverlay.IsVisible)
                    {
                        controlsOverlay.IsVisible = false;
                    }
                    else
                    {
                        OnCloseClicked(this, EventArgs.Empty);
                    }
                    break;
            }
        }
    }
}
