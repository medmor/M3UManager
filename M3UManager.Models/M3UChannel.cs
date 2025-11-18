using System.Text.RegularExpressions;
using M3UManager.Models.XtreamModels;

namespace M3UManager.Models
{
    public enum ContentType
    {
        LiveTV,
        Movie,
        Series,
        Unknown
    }

    public class M3UChannel
    {
        private readonly Regex regexChannelName = new Regex("tvg-name=\"(.*?)\"");
        private readonly Regex regexChannelLogo = new Regex("tvg-logo=\"(.*?)\"");
        public string Name { get; set; }
        public string Url { get; set; }
        public string Group { get; set; }
        public string Logo { get; set; }
        public string FullChannelString { get; set; }
        public int StreamId { get; set; }
        public string CategoryId { get; set; }
        public ContentType Type { get; set; } = ContentType.Unknown;

        public M3UChannel() { }

        public M3UChannel(string channelSring, string group)
        {
            Name = regexChannelName.Match(channelSring).Groups[1].Value;
            Logo = regexChannelLogo.Match(channelSring).Groups[1].Value;
            // Extract URL more efficiently than Split: take the substring after the first newline
            var firstNewline = channelSring.IndexOf('\n');
            Url = firstNewline >= 0 && firstNewline + 1 < channelSring.Length
                ? channelSring.Substring(firstNewline + 1).TrimEnd('\r', '\n')
                : string.Empty;
            FullChannelString = channelSring;
            Group = group;
        }

        // Constructor for Xtream Codes API data
        public M3UChannel(XtreamChannel xtreamChannel, string groupName, string streamUrl, ContentType contentType)
        {
            Name = xtreamChannel.Name;
            Url = streamUrl;
            Group = groupName;
            Logo = xtreamChannel.StreamIcon;
            StreamId = xtreamChannel.StreamId;
            CategoryId = xtreamChannel.CategoryId;
            Type = contentType;
            // Generate a fake EXTINF string for compatibility
            FullChannelString = $"-1 tvg-id=\"{xtreamChannel.EpgChannelId}\" tvg-name=\"{xtreamChannel.Name}\" tvg-logo=\"{xtreamChannel.StreamIcon}\" group-title=\"{groupName}\",{xtreamChannel.Name}\n{streamUrl}";
        }
    }
}
