namespace M3UManager.Models
{
    public class M3UGroup
    {
        public string Name { get; set; } = string.Empty;
        public string TrimedName { get; set; } = string.Empty;
        public List<M3UChannel> Channels { get; set; } = new List<M3UChannel>();

        // Parameterless constructor for JSON deserialization
        public M3UGroup() { }

        public M3UGroup(string name, List<M3UChannel> channels)
        {
            Name = name;
            TrimedName = Utils.TrimmedString(name);
            Channels = channels;
        }
        
        public M3UGroup(string name, IGrouping<string, string> group)
        {
            Name = name;
            TrimedName = Utils.TrimmedString(name);
            Channels = group.Select(c => new M3UChannel(c, group.Key)).ToList();
        }
        
        public void AddChannel(M3UChannel channel)
        {
            if (Channels.Contains(channel)) return;
            Channels.Add(channel);
        }
        public void AddChannels(List<M3UChannel> channels) => Channels.AddRange(channels);
        public void RemoveChannel(M3UChannel channel) => Channels.Remove(channel);
        public void RemoveChannel(string fullChannelString)
        {
            Channels = Channels.Where(c => c.FullChannelString != fullChannelString).ToList();
        }
        public void RemoveChannels(List<string> channelsNames)
        {
            Channels = Channels.Where(c => !channelsNames.Contains(c.Name)).ToList();
        }
    }
}
