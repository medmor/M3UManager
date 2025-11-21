using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;

namespace M3UManager.Views
{
    public partial class PlayerWindow : ContentPage
    {
        public PlayerWindow(string streamUrl, string channelName)
        {
            InitializeComponent();
            
            channelNameLabel.Text = channelName;
            streamUrlLabel.Text = streamUrl;
            
            mediaElement.Source = MediaSource.FromUri(streamUrl);
            
            // Handle media events
            mediaElement.StateChanged += OnMediaStateChanged;
            mediaElement.MediaFailed += OnMediaFailed;
            mediaElement.MediaOpened += OnMediaOpened;
        }

        private void OnMediaOpened(object? sender, EventArgs e)
        {
            // Media successfully opened
            Console.WriteLine("Media opened successfully");
        }

        private void OnMediaFailed(object? sender, MediaFailedEventArgs e)
        {
            // Handle media failure
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Error", $"Failed to load media: {e.ErrorMessage}", "OK");
            });
        }

        private void OnMediaStateChanged(object? sender, MediaStateChangedEventArgs e)
        {
            Console.WriteLine($"Media state changed: {e.NewState}");
            
            if (e.NewState == MediaElementState.Failed)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Playback Error", "The stream failed to play. Please check the stream URL.", "OK");
                });
            }
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            // Stop and dispose media
            mediaElement.Stop();
            mediaElement.Handler?.DisconnectHandler();
            
            // Close the window this page is in
            var window = this.GetParentWindow();
            if (window != null)
            {
                Application.Current?.CloseWindow(window);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Clean up media element
            mediaElement.Stop();
            mediaElement.Handler?.DisconnectHandler();
        }
    }
}
