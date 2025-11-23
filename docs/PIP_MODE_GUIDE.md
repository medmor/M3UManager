# Picture-in-Picture (PiP) Mode - Usage Guide

## ?? What is PiP Mode?

Picture-in-Picture mode allows you to watch a channel in a small, floating player while browsing other channels or managing your playlists. The PiP player stays on top of the main window and can be moved and resized.

## ?? How to Use PiP Mode

### Starting PiP Mode

**Option 1: From Full Player Window**
1. Open a channel in the full player window
2. Click the **"PiP Mode"** button in the control bar
3. The player will minimize to a floating window

**Option 2: Directly from Blazor (Coming Soon)**
- Right-click on a channel ? Select "Open in PiP"

### PiP Controls

#### Moving the PiP Player
- **Click and drag** the bottom bar to move the player anywhere on the screen
- The player will **snap to edges** when moved close to the screen borders

#### Resizing the PiP Player
- **Click and drag** the resize indicator (??) in the bottom-right corner
- Minimum size: 200x150 pixels
- Maximum size: 800x600 pixels

#### Playback Controls
1. **Tap the player** to show/hide controls
2. **Play/Pause button**: Center button (auto-hides after 3 seconds)
3. **Volume/Mute**: Bottom-left button
4. **Expand to Full Window**: Bottom-right expand button (?)
5. **Close PiP**: Bottom-right close button (?)

### Expanding to Full Window
- Click the **expand button** (?) in the PiP controls
- The stream will open in a new full-size player window
- The PiP player will automatically close

### Switching Between Modes

| From | To | How |
|------|-----|-----|
| Full Player | PiP | Click "PiP Mode" button |
| PiP | Full Player | Click expand button (?) |
| Full Player | Another Full Player | Open another channel |
| PiP | Another PiP | Close current, open new |

## ?? Keyboard Shortcuts (Coming Soon)

- `P`: Toggle PiP mode
- `Esc`: Close PiP
- `Space`: Play/Pause (when PiP is focused)
- `M`: Mute/Unmute

## ?? Features

? **Implemented:**
- Draggable positioning
- Resizable window
- Auto-hide controls (3-second timer)
- Play/Pause control
- Volume/Mute control
- Expand to full window
- Snap to edges
- Always on top of main window

?? **Coming Soon:**
- Picture quality selection
- Opacity control
- Remember last position
- Multiple PiP windows
- Keyboard shortcuts
- Touch gestures (mobile)

## ?? Tips

1. **Position at corners**: Drag the PiP to any corner for easy multitasking
2. **Resize for your needs**: Make it smaller for background viewing or larger for focus
3. **Tap to control**: Controls auto-hide to minimize distraction
4. **Quick expansion**: Easily switch to full-screen with one click

## ?? Troubleshooting

**PiP player not appearing:**
- Make sure the stream URL is valid
- Check if MediaElement is properly initialized

**Controls not showing:**
- Tap the player window to toggle controls
- Controls will auto-hide after 3 seconds

**Can't move the player:**
- Make sure you're dragging the bottom bar (dark gray area)
- Check that the player is not at screen edge limits

**Performance issues:**
- Reduce PiP window size for better performance
- Close unused player windows
- Check network connection quality

## ?? Technical Details

- **Framework**: .NET MAUI with CommunityToolkit.Maui.MediaElement
- **Positioning**: AbsoluteLayout with proportional coordinates
- **Gesture Support**: PanGestureRecognizer for drag and resize
- **Auto-hide**: System.Timers.Timer (3-second delay)
- **Always on top**: Implemented via overlay on main window

## ?? Platform Support

| Platform | Support | Notes |
|----------|---------|-------|
| Windows | ? Full | All features supported |
| Android | ?? Partial | May need platform-specific adjustments |
| iOS | ?? Partial | System PiP may interfere |
| macOS | ?? Partial | Requires testing |

---

**Note**: This is a new feature in development. Please report any issues on GitHub!
