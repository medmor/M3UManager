using System;

namespace M3UManager.Models
{
    public class WatchHistory
    {
        public string ChannelName { get; set; } = string.Empty;
        public string ChannelUrl { get; set; } = string.Empty;
        public string ChannelLogo { get; set; } = string.Empty;
        public string ChannelGroup { get; set; } = string.Empty;
        public DateTime LastWatched { get; set; }
        public TimeSpan LastPosition { get; set; }
        public ContentType ContentType { get; set; }
        
        // For Xtream channels
        public int StreamId { get; set; }
        public string? CategoryId { get; set; }
        public string? XtreamServerUrl { get; set; }
        public string? XtreamUsername { get; set; }
        public string? XtreamPassword { get; set; }

        public WatchHistory() { }

        public WatchHistory(M3UChannel channel, TimeSpan position)
        {
            ChannelName = channel.Name;
            ChannelUrl = channel.Url;
            ChannelLogo = channel.Logo;
            ChannelGroup = channel.Group;
            LastWatched = DateTime.Now;
            LastPosition = position;
            ContentType = channel.Type;
            StreamId = channel.StreamId;
            CategoryId = channel.CategoryId;
            XtreamServerUrl = channel.XtreamServerUrl;
            XtreamUsername = channel.XtreamUsername;
            XtreamPassword = channel.XtreamPassword;
        }

        public M3UChannel ToChannel()
        {
            return new M3UChannel
            {
                Name = ChannelName,
                Url = ChannelUrl,
                Logo = ChannelLogo,
                Group = ChannelGroup,
                Type = ContentType,
                StreamId = StreamId,
                CategoryId = CategoryId,
                XtreamServerUrl = XtreamServerUrl,
                XtreamUsername = XtreamUsername,
                XtreamPassword = XtreamPassword
            };
        }
    }
}
