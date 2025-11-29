namespace M3UManager.Models
{
    public class FavoriteCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
        public ICollection<M3UChannel> Channels { get; set; }

        public FavoriteCategory()
        {
            Id = Guid.NewGuid().ToString();
            Name = "Untitled";
            Icon = "bi-star";
            IsDefault = false;
            SortOrder = 0;
            Channels = new List<M3UChannel>();
        }

        public FavoriteCategory(string name, string icon = "bi-star", bool isDefault = false, int sortOrder = 0)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Icon = icon;
            IsDefault = isDefault;
            SortOrder = sortOrder;
            Channels = new List<M3UChannel>();
        }

        public void AddChannel(M3UChannel channel)
        {
            if (!Channels.Any(c => c.FullChannelString == channel.FullChannelString))
            {
                Channels.Add(channel);
            }
        }

        public void RemoveChannel(string channelFullString)
        {
            var channel = Channels.FirstOrDefault(c => c.FullChannelString == channelFullString);
            if (channel != null)
            {
                Channels.Remove(channel);
            }
        }

        public bool ContainsChannel(M3UChannel channel)
        {
            return Channels.Any(c => c.FullChannelString == channel.FullChannelString);
        }
    }
}
