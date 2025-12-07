using M3UManager.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace M3UManager.UI.Components
{
    public enum DisplayMode
    {
        List,
        Grid
    }

    public partial class ChannelsDisplay
    {
        [Parameter] public List<M3UChannel>? Channels { get; set; }
        [Parameter] public M3UChannel? SelectedChannel { get; set; }
        [Parameter] public EventCallback<M3UChannel> OnChannelSelected { get; set; }
        [Parameter] public EventCallback<M3UChannel> OnPlay { get; set; }
        [Parameter] public EventCallback<M3UChannel> OnPlayPip { get; set; }
        [Parameter] public EventCallback<M3UChannel> OnToggleFavorite { get; set; }
        [Parameter] public Func<M3UChannel, bool>? IsChannelFavorite { get; set; }
        [Parameter] public bool ShowSearch { get; set; } = true;
        [Parameter] public bool ShowActions { get; set; } = true;
        [Parameter] public DisplayMode InitialViewMode { get; set; } = DisplayMode.List;
        [Parameter] public bool UseImageProxy { get; set; } = true;
        [Parameter] public EventCallback<M3UChannel> OnShowEpisodes { get; set; }

        private DisplayMode ViewMode { get; set; }
        private List<M3UChannel> FilteredChannels { get; set; } = new();
        private string searchText = string.Empty;
        private const string VIEW_MODE_PREFERENCE_KEY = "ChannelsDisplayViewMode";

        protected override void OnInitialized()
        {
            // Load saved view mode preference
            var savedViewMode = GetViewModePreference();
            ViewMode = savedViewMode ?? InitialViewMode;
            
            UpdateFilteredChannels();
        }

        protected override void OnParametersSet()
        {
            UpdateFilteredChannels();
        }

        private void FilterChannels(ChangeEventArgs e)
        {
            searchText = e.Value?.ToString() ?? string.Empty;
            UpdateFilteredChannels();
        }

        private void UpdateFilteredChannels()
        {
            if (Channels == null)
            {
                FilteredChannels = new List<M3UChannel>();
                return;
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                FilteredChannels = Channels.ToList();
            }
            else
            {
                var searchTerms = searchText.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                FilteredChannels = Channels
                    .Select(channel => new { Channel = channel, Score = CalculateSearchScore(channel, searchTerms) })
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .Select(x => x.Channel)
                    .ToList();
            }
            
            StateHasChanged();
        }

        private int CalculateSearchScore(M3UChannel channel, string[] searchTerms)
        {
            var score = 0;
            var name = channel.Name?.ToLower() ?? string.Empty;
            var group = channel.Group?.ToLower() ?? string.Empty;

            foreach (var term in searchTerms)
            {
                // Exact match in name gets highest score
                if (name == term)
                    score += 100;
                // Name starts with term
                else if (name.StartsWith(term))
                    score += 50;
                // Name contains term
                else if (name.Contains(term))
                    score += 30;
                // Fuzzy match in name (allows for typos)
                else if (FuzzyMatch(name, term))
                    score += 15;

                // Group matches
                if (group == term)
                    score += 40;
                else if (group.Contains(term))
                    score += 20;

                // Check individual words in name
                var nameWords = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in nameWords)
                {
                    if (word.StartsWith(term))
                        score += 25;
                }
            }

            return score;
        }

        private bool FuzzyMatch(string source, string term)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(term))
                return false;

            // Simple fuzzy matching: check if characters appear in order
            var sourceIndex = 0;
            var termIndex = 0;

            while (sourceIndex < source.Length && termIndex < term.Length)
            {
                if (source[sourceIndex] == term[termIndex])
                    termIndex++;
                sourceIndex++;
            }

            return termIndex == term.Length;
        }

        private IEnumerable<M3UChannel> GetVisibleChannels()
        {
            return FilteredChannels;
        }

        private void ClearSearch()
        {
            searchText = string.Empty;
            UpdateFilteredChannels();
        }

        private async Task SelectChannel(M3UChannel channel)
        {
            SelectedChannel = channel;
            await OnChannelSelected.InvokeAsync(channel);
            
            // If it's a TV show and OnShowEpisodes is set, trigger episode viewing
            if (channel.Type == ContentType.Series && OnShowEpisodes.HasDelegate)
            {
                await OnShowEpisodes.InvokeAsync(channel);
            }
            
            StateHasChanged();
        }

        private string GetChannelClass(M3UChannel channel)
        {
            return SelectedChannel == channel ? "selected" : string.Empty;
        }

        private void ChangeViewMode(DisplayMode mode)
        {
            ViewMode = mode;
            SaveViewModePreference(mode);
            StateHasChanged();
        }

        private void SaveViewModePreference(DisplayMode mode)
        {
            try
            {
                Preferences.Set(VIEW_MODE_PREFERENCE_KEY, mode.ToString());
            }
            catch
            {
                // Silently ignore if preferences can't be saved
            }
        }

        private DisplayMode? GetViewModePreference()
        {
            try
            {
                var saved = Preferences.Get(VIEW_MODE_PREFERENCE_KEY, string.Empty);
                if (Enum.TryParse<DisplayMode>(saved, out var mode))
                {
                    return mode;
                }
            }
            catch
            {
                // Silently ignore if preferences can't be loaded
            }
            return null;
        }

        /// <summary>
        /// Handles image URL proxying and security issues
        /// Converts HTTP to HTTPS where possible and uses proxy services for problematic URLs
        /// </summary>
        private string GetProxiedImageUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return string.Empty;

            // Don't proxy if disabled
            if (!UseImageProxy)
                return imageUrl;

            try
            {
                // Parse the URL
                if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
                    return imageUrl;

                // If it's already HTTPS, return as-is
                if (uri.Scheme == "https")
                    return imageUrl;

                // Try to upgrade HTTP to HTTPS for known safe domains
                if (uri.Scheme == "http")
                {
                    var safeDomains = new[]
                    {
                        "pbs.twimg.com",
                        "i.imgur.com",
                        "upload.wikimedia.org",
                        "image.tmdb.org"
                    };

                    if (safeDomains.Any(d => uri.Host.Contains(d, StringComparison.OrdinalIgnoreCase)))
                    {
                        return $"https://{uri.Host}{uri.PathAndQuery}";
                    }

                    // For other HTTP URLs, use image proxy services
                    // These are free services that handle HTTPS, CORS, and caching
                    
                    // Option 1: images.weserv.nl - Reliable free image proxy
                    var escapedUrl = Uri.EscapeDataString(imageUrl);
                    return $"https://images.weserv.nl/?url={escapedUrl}&w=400&h=400&fit=cover&default=1";
                    
                    // Option 2: wsrv.nl (alternative)
                    // return $"https://wsrv.nl/?url={escapedUrl}&w=400&h=400&fit=cover&default=1";
                }

                return imageUrl;
            }
            catch
            {
                // If any error occurs, return the original URL
                return imageUrl;
            }
        }

        // Public method to update channels from parent
        public void UpdateChannels(List<M3UChannel> channels)
        {
            Channels = channels;
            UpdateFilteredChannels();
            StateHasChanged();
        }
    }
}
