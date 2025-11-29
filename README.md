# M3 Manager - IPTV Player & Manager

A modern cross-platform IPTV player and playlist manager built with .NET MAUI and Blazor. Manage your M3U playlists, organize channels, and watch live TV with an integrated media player.

## ?? Features

### ? Current Features
- **M3U Playlist Support**: Import playlists from files or URLs
- **Xtream Codes API Integration**: Direct connection to IPTV providers
- **Content Organization**: Automatic categorization (Live TV, Movies, TV Shows)
- **Favorites Management**: Save and manage your favorite channels
- **Integrated Media Player**: Native MediaElement player with independent windows
- **Multi-Window Support**: Browse channels while watching (Windows)
- **Picture-in-Picture Mode**: Minimized playback overlay with draggable player
- **Search & Filter**: Quick search across channels and groups
- **Playlist Cache**: Offline access to playlist data
- **Channel Grid & List Views**: Multiple viewing layouts
- **Series Episode Viewer**: Browse and play TV show episodes

## ??? Technology Stack

- **.NET 9**
- **.NET MAUI** (Cross-platform UI)
- **Blazor** (Web components)
- **CommunityToolkit.Maui.MediaElement** (Media playback)
- **SQLite** (Local data storage)

## ?? TODO List

### ?? High Priority

#### Player Enhancements
- [x] **Picture-in-Picture (PiP) Mode**
  - [x] Allow minimized playback overlay
  - [x] Draggable mini-player on main window
  - [x] Resizable PiP window
  - [x] Expand to full window
  - [x] Auto-hide controls
  
- [x] **Enhanced Playback Controls**
  - [x] Add progress bar with timeline
  - [x] Volume slider with mute button
  - [x] Display current time / total duration
  - [x] Add 10s forward/backward buttons
  - [x] Implement keyboard shortcuts (handlers ready):
    - Space: Play/Pause
    - Arrow Left/Right: Seek -10s/+10s
    - Arrow Up/Down: Volume up/down
    - F: Fullscreen toggle
    - M: Mute toggle
    - Esc: Exit fullscreen/close player
  - [x] Auto-hide controls overlay
  - [x] Tap to show/hide controls

- [ ] **Stream Quality Selection**
  - Detect available quality levels
  - Manual quality selector (Auto/SD/HD/FHD)
  - Bandwidth-based auto-selection

- [ ] **Audio & Subtitle Support**
  - [ ] Multiple audio track selection
  - [ ] Subtitle file loading (.srt, .ass)
  - [ ] Subtitle styling options

#### EPG (Electronic Program Guide)
- [ ] **EPG Integration**
  - [ ] Add EPG service interface
  - [ ] Parse XMLTV format
  - [ ] Fetch EPG from Xtream API
  - [ ] Display current/next program info
  - [ ] Show program descriptions
  - [ ] Implement EPG timeline view
  - [ ] Add "What's On Now" section
  - [ ] Enable catch-up TV viewing

#### Favorites Management
- [x] **Multiple Favorite Lists**
  - [x] Create custom favorite categories
  - [x] "Sports", "News", "Movies" default lists
  - [ ] Drag-and-drop between lists
  
- [ ] **Favorites Organization**
  - [ ] Drag-and-drop reordering
  - [ ] Number shortcuts (1-9) for quick access
  - [ ] Export/Import favorites
  - [ ] Share favorites as M3U file

- [ ] **Recently Watched**
  - [ ] Auto-populate viewing history
  - [ ] Clear history option
  - [ ] Resume from last position

### ?? UI/UX Improvements

#### Visual Enhancements
- [ ] **Themes System**
  - [ ] Light theme
  - [ ] Dark theme (default)
  - [ ] System theme (follow OS)
  - [ ] Custom accent colors
  - [ ] Theme persistence

- [ ] **Channel Display**
  - [ ] Improve grid view layout
  - [ ] Better logo caching
  - [ ] Hover effects with quick info
  - [ ] Genre/Category badges
  - [ ] Channel numbers display

- [ ] **Mini Player Control Bar**
  - [ ] Persistent control bar when minimized
  - [ ] Quick channel info overlay
  - [ ] Auto-hide overlay after 3 seconds

#### Search & Navigation
- [ ] **Smart Search**
  - [ ] Fuzzy search algorithm
  - [ ] Search across all fields (name, group, description)
  - [ ] Search history
  - [ ] Recent searches dropdown
  - [ ] Clear search history

- [ ] **Advanced Filtering**
  - [ ] Filter by content type
  - [ ] Filter by genre/category
  - [ ] Filter by provider
  - [ ] Custom filter combinations

- [ ] **Channel Navigation**
  - [ ] Navigate by channel numbers
  - [ ] Channel up/down buttons
  - [ ] Jump to channel by number input

### ?? Security & Parental Controls

- [ ] **Parental Controls**
  - [ ] PIN code setup
  - [ ] Lock/unlock channels
  - [ ] Block categories (e.g., Adult)
  - [ ] Time-based restrictions
  - [ ] Age ratings support

- [ ] **Privacy**
  - [ ] Secure credential storage
  - [ ] Remember login option
  - [ ] Auto-lock on minimize

### ?? Data & Performance

#### Performance Optimization
- [ ] **Buffering & Caching**
  - [ ] Pre-buffer streams for instant playback
  - [ ] Cache channel logos locally
  - [ ] Optimize image loading
  - [ ] Lazy load channel lists

- [ ] **Network Optimization**
  - [ ] Detect network speed
  - [ ] Auto-adjust quality based on bandwidth
  - [ ] Show network status indicator
  - [ ] Offline mode for browsing

- [ ] **Error Recovery**
  - [ ] Auto-retry failed streams (3 attempts)
  - [ ] Fallback to alternate URLs
  - [ ] Better error messages
  - [ ] Connection diagnostics tool

#### Playlist Management
- [ ] **Advanced Playlist Features**
  - [ ] Import from multiple formats (M3U, M3U8, XSPF, PLS)
  - [ ] Export playlists
  - [ ] Merge multiple playlists
  - [ ] Remove duplicate channels
  - [ ] Playlist auto-refresh
  - [ ] Schedule playlist updates

- [ ] **Multi-Provider Support**
  - [ ] Support multiple IPTV providers
  - [ ] Quick switch between providers
  - [ ] Provider-specific settings
  - [ ] Provider management UI

### ?? Advanced Features

#### Recording & DVR
- [ ] **Recording Service**
  - [ ] Record current stream
  - [ ] Schedule recordings
  - [ ] Set recording duration
  - [ ] Recording quality settings
  - [ ] Manage recorded files
  - [ ] Playback recorded content

#### Series Management
- [ ] **Enhanced Series Features**
  - [ ] Track watched episodes
  - [ ] "Continue Watching" with progress
  - [ ] Auto-play next episode
  - [ ] Season/Episode progress bars
  - [ ] Mark as watched/unwatched
  - [ ] Series notifications

#### Casting & Streaming
- [ ] **Chromecast Support**
  - [ ] Discover Chromecast devices
  - [ ] Cast stream to device
  - [ ] Remote playback controls
  
- [ ] **DLNA Support**
  - [ ] Discover DLNA devices
  - [ ] Stream to smart TVs
  - [ ] Network media sharing

#### Analytics & Insights
- [ ] **Viewing Statistics**
  - [ ] Most watched channels
  - [ ] Total watch time
  - [ ] Data usage tracking
  - [ ] Viewing history
  - [ ] Generate reports

- [ ] **Dashboard**
  - [ ] Statistics overview
  - [ ] Quick access widgets
  - [ ] Recommendations based on history

### ?? Notifications & Alerts

- [ ] **Smart Notifications**
  - [ ] Favorite show starting soon
  - [ ] Recording completed
  - [ ] New episodes available
  - [ ] Playlist updated
  - [ ] System notifications integration

### ?? Localization

- [ ] **Multi-Language Support**
  - [ ] English (default)
  - [ ] French
  - [ ] Arabic (with RTL support)
  - [ ] Spanish
  - [ ] German
  - [ ] Add more languages
  - [ ] Language-specific content filtering

### ?? Backup & Settings

- [ ] **Settings Management**
  - [ ] Player settings panel
  - [ ] Network settings
  - [ ] UI preferences
  - [ ] Keybindings customization
  - [ ] Import/Export settings

- [ ] **Backup & Sync**
  - [ ] Backup favorites & settings
  - [ ] Restore from backup
  - [ ] Cloud sync (OneDrive, Google Drive)
  - [ ] Automatic backups

### ?? Platform-Specific

#### Windows
- [ ] **Windows Integration**
  - [ ] System tray support
  - [ ] Minimize to tray
  - [ ] Global hotkeys
  - [ ] Jump list integration
  - [ ] Windows 11 snap layouts support

#### Android
- [ ] **Android Optimization**
  - [ ] Mobile-friendly UI
  - [ ] Touch gestures (swipe, pinch-to-zoom)
  - [ ] Mobile data usage warnings
  - [ ] Battery optimization mode
  - [ ] Background playback

#### Android TV
- [ ] **Android TV Support**
  - [ ] 10-foot UI (TV-optimized interface)
  - [ ] D-pad navigation
  - [ ] Remote control support
  - [ ] Leanback mode

### ?? Testing & Quality

- [ ] **Unit Tests**
  - [ ] Service layer tests
  - [ ] M3U parser tests
  - [ ] Xtream API tests
  - [ ] Mock provider for testing

- [ ] **Integration Tests**
  - [ ] End-to-end scenarios
  - [ ] Multi-platform tests

- [ ] **Logging & Diagnostics**
  - [ ] Implement logging service
  - [ ] Log stream errors
  - [ ] Export logs feature
  - [ ] Crash reporting
  - [ ] Performance monitoring

### ?? Quick Wins (Easy Implementations)

- [ ] Add channel numbers to listings
- [ ] Last played channel on startup
- [ ] Remember volume level
- [ ] Remember player window size/position
- [ ] Channel logo fallback icons
- [ ] Loading indicators
- [ ] Empty state messages
- [ ] Tooltips for buttons
- [ ] Confirmation dialogs for delete actions
- [ ] Keyboard focus indicators

## ?? Documentation

- [ ] **User Documentation**
  - [ ] User guide
  - [ ] Installation instructions
  - [ ] Feature walkthrough
  - [ ] FAQ section
  - [ ] Troubleshooting guide

- [ ] **Developer Documentation**
  - [ ] Architecture overview
  - [ ] API documentation
  - [ ] Contributing guidelines
  - [ ] Code style guide
  - [ ] Build instructions

## ?? Roadmap

### Phase 1: Core Functionality (v1.1)
- EPG Integration
- Enhanced Favorites
- Better Error Handling
- Settings Management
- Recording/DVR basics

### Phase 2: User Experience (v1.2)
- Themes & Customization
- Smart Search
- Keyboard Shortcuts
- Notifications
- Analytics Dashboard

### Phase 3: Advanced Features (v2.0)
- Chromecast/DLNA Support
- Android TV Support
- Multi-language
- Cloud Sync
- Series Management

### Phase 4: Platform Expansion (v2.1+)
- iOS Support
- macOS Support
- Linux Support
- Web Player

## ?? Contributing

Contributions are welcome! Please read the contributing guidelines before submitting pull requests.

## ?? License

[Specify your license here]

## ?? Links

- GitHub Repository: https://github.com/medmor/M3UManager
- Issue Tracker: https://github.com/medmor/M3UManager/issues
- Discussions: https://github.com/medmor/M3UManager/discussions

## ?? Contact

[Your contact information]

---

**Note**: This is an active project under development. Features marked with ? are implemented, others are planned.