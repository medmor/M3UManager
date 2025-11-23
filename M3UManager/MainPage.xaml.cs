using M3UManager.Views;

namespace M3UManager;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // Subscribe to PiP events
        pipPlayer.ExpandRequested += OnPipExpandRequested;
        pipPlayer.PipClosed += OnPipClosed;
    }

    private async void OnPipExpandRequested(object? sender, PipExpandedEventArgs e)
    {
        // Open full player window when user expands from PiP
        var playerWindow = new PlayerWindow(e.StreamUrl, e.ChannelName);

        var newWindow = new Window(playerWindow)
        {
            Title = "Media Player",
            Width = 850,
            Height = 650
        };

        Application.Current?.OpenWindow(newWindow);
    }

    private void OnPipClosed(object? sender, EventArgs e)
    {
        // Handle PiP close if needed
        Console.WriteLine("PiP player closed");
    }

    // Public method to open PiP from Blazor or other components
    public void OpenPipPlayer(string streamUrl, string channelName)
    {
        pipPlayer.LoadStream(streamUrl, channelName);
    }
}
