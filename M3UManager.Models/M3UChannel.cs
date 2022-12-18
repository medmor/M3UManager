namespace M3UManager.Models
{
    public class M3UChannel
    {

        public string Name { get; set; }
        public string Url { get; set; }
        public string Group { get; set; }
        public string Logo { get; set; }
        public string FullChannelString { get; set; }

        public M3UChannel() { }

        public M3UChannel(string channelSring, string group)
        {
            Name = Utils.RegexChannelName.Match(channelSring).Groups[1].Value;
            Logo = Utils.RegexChannelLogo.Match(channelSring).Groups[1].Value;
            Url = channelSring.Split('\n')[1];
            FullChannelString = channelSring;
            Group = group;
        }
    }
}
