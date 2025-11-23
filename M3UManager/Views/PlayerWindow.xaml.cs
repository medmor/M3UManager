using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Views
{
    public partial class PlayerWindow : ContentPage
    {
        private readonly string _streamUrl;
        private readonly string _channelName;
        private bool _isPlaying = true;
        private bool _isSeeking = false;
        private System.Timers.Timer? _progressTimer;
        private System.Timers.Timer? _controlsTimer;

        public PlayerWindow(string streamUrl, string channelName)
        {
            InitializeComponent();
            
            _streamUrl = streamUrl;
            _channelName = channelName;
            
            channelNameLabel.Text = channelName;
            overlayChannelName.Text = channelName;
            streamUrlLabel.Text = streamUrl;
            
            mediaElement.Source = MediaSource.FromUri(streamUrl);
            
            // Handle media events
            mediaElement.StateChanged += OnMediaStateChanged;
            mediaElement.MediaFailed += OnMediaFailed;
            mediaElement.MediaOpened += OnMediaOpened;
            mediaElement.PositionChanged += OnPositionChanged;

            // Setup progress update timer
            _progressTimer = new System.Timers.Timer(500); // Update every 500ms
            _progressTimer.Elapsed += UpdateProgressUI;
            _progressTimer.Start();

            // Setup controls auto-hide timer
            _controlsTimer = new System.Timers.Timer(3000); // 3 seconds
            _controlsTimer.Elapsed += (s, e) => MainThread.BeginInvokeOnMainThread(() =>
            {
                controlsOverlay.IsVisible = false;
            });
            _controlsTimer.Start();

            // Add keyboard event handler
            this.Loaded += OnPageLoaded;
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

        private void OnPlayerTapped(object? sender, TappedEventArgs e)
        {
            // Toggle controls visibility
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
            _controlsTimer?.Stop();
            _controlsTimer?.Dispose();

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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Cleanup
            _progressTimer?.Stop();
            _progressTimer?.Dispose();
            _controlsTimer?.Stop();
            _controlsTimer?.Dispose();
            
            mediaElement.Stop();
            mediaElement.Handler?.DisconnectHandler();
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
