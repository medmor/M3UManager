using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace M3UManager.Views
{
    public partial class PipPlayerView : ContentView
    {
        private double _currentX;
        private double _currentY;
        private double _currentWidth = 320;
        private double _currentHeight = 200;
        private bool _isPlaying = true;
        private bool _isSeeking = false;
        private System.Timers.Timer? _controlsTimer;
        private System.Timers.Timer? _progressTimer;

        public event EventHandler<PipExpandedEventArgs>? ExpandRequested;
        public event EventHandler? PipClosed;

        public PipPlayerView()
        {
            InitializeComponent();
            
            // Initialize position (will be set when shown based on window size)
            _currentX = 0;
            _currentY = 0;

            // Setup controls auto-hide timer
            _controlsTimer = new System.Timers.Timer(3000); // 3 seconds
            _controlsTimer.Elapsed += (s, e) => MainThread.BeginInvokeOnMainThread(() =>
            {
                controlsOverlay.IsVisible = false;
            });

            // Setup progress update timer
            _progressTimer = new System.Timers.Timer(500); // Update every 500ms
            _progressTimer.Elapsed += UpdateProgressUI;
            
            // Listen to size changes to reposition PIP
            this.SizeChanged += OnPipViewSizeChanged;
        }

        private void OnPipViewSizeChanged(object? sender, EventArgs e)
        {
            // Position PIP at bottom-right corner when window size changes
            if (this.Width > 0 && this.Height > 0)
            {
                _currentX = this.Width - _currentWidth - 20; // 20px margin
                _currentY = this.Height - _currentHeight - 20;
                
                AbsoluteLayout.SetLayoutBounds(pipContainer,
                    new Rect(_currentX, _currentY, _currentWidth, _currentHeight));
            }
        }

        public void LoadStream(string streamUrl, string channelName)
        {
            channelNameLabel.Text = channelName;
            pipChannelLabel.Text = channelName;
            mediaElement.Source = MediaSource.FromUri(streamUrl);
            
            // Handle media events
            mediaElement.StateChanged += OnMediaStateChanged;
            mediaElement.MediaFailed += OnMediaFailed;
            mediaElement.MediaOpened += OnMediaOpened;
            mediaElement.PositionChanged += OnPositionChanged;

            // Show the PiP player
            ((ContentView)this).IsVisible = true;
            
            // Start progress timer
            _progressTimer?.Start();
        }

        public void Show()
        {
            ((ContentView)this).IsVisible = true;
            _progressTimer?.Start();
        }

        public void Hide()
        {
            ((ContentView)this).IsVisible = false;
            _progressTimer?.Stop();
            _controlsTimer?.Stop();
            mediaElement.Stop();
        }

        private void OnMediaOpened(object? sender, EventArgs e)
        {
            Console.WriteLine("PiP: Media opened successfully");
            _isPlaying = true;
            UpdatePlayPauseIcon();

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
                await Application.Current?.MainPage?.DisplayAlert("PiP Error", 
                    $"Failed to load stream: {e.ErrorMessage}", "OK")!;
            });
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
            mediaElement.SeekTo(newPosition);
        }

        private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (sender is not Grid) return;

            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    var parentWidth = ((ContentView)this).Width;
                    var parentHeight = ((ContentView)this).Height;

                    if (parentWidth > 0 && parentHeight > 0)
                    {
                        _currentX += e.TotalX;
                        _currentY += e.TotalY;

                        // Clamp to window bounds
                        _currentX = Math.Clamp(_currentX, 0, parentWidth - _currentWidth);
                        _currentY = Math.Clamp(_currentY, 0, parentHeight - _currentHeight);

                        AbsoluteLayout.SetLayoutBounds(pipContainer,
                            new Rect(_currentX, _currentY, _currentWidth, _currentHeight));
                    }
                    break;

                case GestureStatus.Completed:
                    // Snap to edges if close enough (within 20px)
                    var parentW = ((ContentView)this).Width;
                    var parentH = ((ContentView)this).Height;
                    
                    if (_currentX < 20) _currentX = 0;
                    else if (_currentX > parentW - _currentWidth - 20) _currentX = parentW - _currentWidth;
                    
                    if (_currentY < 20) _currentY = 0;
                    else if (_currentY > parentH - _currentHeight - 20) _currentY = parentH - _currentHeight;

                    AbsoluteLayout.SetLayoutBounds(pipContainer,
                        new Rect(_currentX, _currentY, _currentWidth, _currentHeight));
                    break;
            }
        }

        private void OnResizePanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (e.StatusType == GestureStatus.Running)
            {
                var parentWidth = ((ContentView)this).Width;
                var parentHeight = ((ContentView)this).Height;
                
                _currentWidth = Math.Clamp(_currentWidth + e.TotalX, 200, Math.Min(800, parentWidth));
                _currentHeight = Math.Clamp(_currentHeight + e.TotalY, 150, Math.Min(600, parentHeight));

                // Ensure PIP doesn't go out of bounds when resizing
                if (_currentX + _currentWidth > parentWidth)
                    _currentX = parentWidth - _currentWidth;
                if (_currentY + _currentHeight > parentHeight)
                    _currentY = parentHeight - _currentHeight;

                AbsoluteLayout.SetLayoutBounds(pipContainer,
                    new Rect(_currentX, _currentY, _currentWidth, _currentHeight));
            }
        }

        private void OnPipTapped(object? sender, TappedEventArgs e)
        {
            controlsOverlay.IsVisible = !controlsOverlay.IsVisible;

            if (controlsOverlay.IsVisible)
            {
                _controlsTimer?.Stop();
                _controlsTimer?.Start();
            }
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

        private void OnVolumeClicked(object? sender, EventArgs e)
        {
            mediaElement.ShouldMute = !mediaElement.ShouldMute;
            
            try
            {
                volumeButton.Text = mediaElement.ShouldMute ? "🔇" : "🔊";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating volume icon: {ex.Message}");
            }
            ResetControlsTimer();
        }

        private void OnExpandClicked(object? sender, EventArgs e)
        {
            var streamUrl = mediaElement.Source?.ToString() ?? string.Empty;
            var channelName = channelNameLabel.Text;

            ExpandRequested?.Invoke(this, new PipExpandedEventArgs(streamUrl, channelName));
            Hide();
        }

        private void OnClosePipClicked(object? sender, EventArgs e)
        {
            Hide();
            PipClosed?.Invoke(this, EventArgs.Empty);
        }

        private void UpdatePlayPauseIcon()
        {
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

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler == null)
            {
                _controlsTimer?.Stop();
                _controlsTimer?.Dispose();
                _progressTimer?.Stop();
                _progressTimer?.Dispose();
                mediaElement.Stop();
            }
        }
    }

    public class PipExpandedEventArgs : EventArgs
    {
        public string StreamUrl { get; }
        public string ChannelName { get; }

        public PipExpandedEventArgs(string streamUrl, string channelName)
        {
            StreamUrl = streamUrl;
            ChannelName = channelName;
        }
    }
}
