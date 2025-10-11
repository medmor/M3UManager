using System.Text.RegularExpressions;

namespace M3UManager.Models
{
    public class M3UChannel
    {
        private readonly Regex regexChannelName = new Regex("tvg-name=\"(.*?)\"");
        private readonly Regex regexChannelLogo = new Regex("tvg-logo=\"(.*?)\"");
        public string Name { get; set; }
        public string Url { get; set; }
        public string Group { get; set; }
        public string Logo { get; set; }
        public string FullChannelString { get; set; }

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
    }
}
