# System-Level Picture-in-Picture Mode

The M3UManager now supports true system-level Picture-in-Picture (PiP) mode that allows the video player to float freely across your entire screen, not restricted to the app window.

## Features

### 🎯 System-Level Floating Window
- PiP window floats on top of all applications
- Not confined to M3UManager app boundaries
- Can be positioned anywhere on your screen
- Works across multiple monitors

### 🎨 Modern UI Design
- Compact 400×280px window
- Netflix-style design with gradient overlays
- Blue accent border (#007ACC)
- Auto-hiding controls overlay

### 🎮 Full Media Controls
- Play/Pause button (center overlay)
- Volume control with mute toggle
- 10-second rewind/forward buttons
- Progress bar with seek support
- Current time and duration display
- Expand to full player button
- Close button

### 🖱️ Interactive Window
- Drag handle at bottom for repositioning
- Resizable window (Windows platform)
- Tap video area to show/hide controls
- Auto-hide controls after 3 seconds

## How to Use

### From Channel List
1. Select any channel in the Editor or Favorites page
2. Click the **PiP** button (blue button with picture-in-picture icon)
3. A floating window opens at position (100, 100)
4. Stream starts playing automatically

### Controls
- **Play/Pause**: Click center of video or play/pause button
- **Rewind/Forward**: Use ⏪ and ⏩ buttons (10-second jumps)
- **Volume**: Click 🔊 to mute/unmute
- **Seek**: Drag the red progress bar
- **Expand**: Click ⛶ to open full player window
- **Close**: Click ✕ to close PiP window

### Repositioning
- Click and drag the bottom bar (with "⋮⋮" handle)
- Window moves smoothly across screen
- Position persists until closed

## Technical Implementation

### Architecture
```
M3UManager.Services.MediaPlayerService
  └─> OpenPipPlayer(streamUrl, channelName)
      └─> Creates PipWindow (ContentPage)
          └─> Wrapped in Window (400×280)
              └─> Application.Current.OpenWindow(window)
```

### Key Components

**PipWindow.xaml** (ContentPage)
- Border with rounded corners
- MediaElement for video playback
- Controls overlay with Grid layout
- Drag handle with PanGestureRecognizer

**PipWindow.xaml.cs**
- Media state management
- Timer-based progress updates
- Auto-hide controls timer
- Event handlers for all controls

**App.xaml.cs** (RegisterPipFactory)
- Creates Window wrapper
- Sets initial size and position
- Handles ExpandRequested event

### UI Components Hierarchy
```
ContentPage (PipWindow)
  └─> Border (rounded, blue stroke)
      └─> Grid
          ├─> MediaElement (fullscreen)
          ├─> Controls Overlay (Grid, auto-hide)
          │   ├─> Channel Name Label
          │   ├─> Play/Pause Button (center)
          │   └─> Bottom Controls
          │       ├─> Progress Slider (red)
          │       └─> Button Bar (⏪⏩🔊⛶✕)
          └─> Drag Handle (bottom bar)
```

## Integration Points

### ChannelsList.razor
```razor
<ChannelsDisplay 
  OnPlayPip="PlayChannelPip"
  ... />
```

### ChannelsList.razor.cs
```csharp
private async Task PlayChannelPip(M3UChannel channel)
{
    await mediaPlayerService.OpenPipPlayer(channel.Url, channel.Name);
}
```

### Favorites Page
Same integration as ChannelsList - PiP button appears when channel selected

## Button Icons

All icons use Unicode emoji characters for maximum compatibility:
- ⏸ - Pause
- ▶ - Play
- 🔊 - Volume On
- 🔇 - Volume Muted
- ⏪ - Rewind 10s
- ⏩ - Forward 10s
- ⛶ - Expand to Full Screen
- ✕ - Close Window

## Differences from Old PiP

### Old Implementation (PipPlayerView)
- ContentView embedded in MainPage Grid
- Absolute positioning within app boundaries
- Could not extend beyond M3UManager window
- Used AbsoluteLayout at (0, 0, 320, 200)

### New Implementation (PipWindow)
- Separate Window with ContentPage
- System-level floating window
- Can be positioned anywhere on screen
- Works across multiple monitors
- Proper window management (minimize, maximize, close)

## Known Limitations

1. **Windows Only**: System-level windows work best on Windows desktop
2. **Manual Positioning**: Initial position is fixed (100, 100)
3. **No Always-on-Top**: MAUI doesn't natively support window z-order control
4. **Single Instance**: Opening new PiP closes any existing PiP window

## Future Enhancements

- [ ] Remember last PiP position
- [ ] Always-on-top using platform-specific APIs
- [ ] Snap-to-edge positioning
- [ ] Multiple simultaneous PiP windows
- [ ] Mini-mode (controls-only, no video)
- [ ] Picture-in-Picture API integration (where supported)

## Testing

1. Open M3UManager in Visual Studio
2. Run the app (F5 or "Windows Machine" profile)
3. Navigate to Editor or Favorites
4. Select a channel with valid stream URL
5. Click the blue PiP button
6. Verify:
   - Window opens at (100, 100)
   - Stream plays automatically
   - Controls overlay appears on tap
   - Controls auto-hide after 3 seconds
   - Window can be dragged
   - Expand button opens full player
   - Close button closes PiP

## Troubleshooting

### PiP Button Not Visible
- Ensure a channel is selected in the list
- Check that `OnPlayPip` callback is wired in .razor file

### Window Not Opening
- Check that `MediaPlayerService.RegisterPipFactory` is called in App.xaml.cs
- Verify no COM exceptions in debug output
- Ensure running as Windows App Runtime (not console)

### Stream Not Playing
- Verify stream URL is valid
- Check that MediaElement can access the network
- Look for errors in debug console

### Window Won't Drag
- Ensure dragging the bottom bar (not video area)
- Check PanGestureRecognizer is attached to dragHandle Grid
- Verify OnDragHandlePan handler is called

## See Also

- [Enhanced Playback Controls](ENHANCED_PLAYBACK_CONTROLS.md)
- [Player Window Guide](../README.md#player-features)
