namespace M3UManager.Data
{
    public class M3UChannel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Group { get; set; }
        public string Logo { get; set; }
        public string FullChannelString { get; set; }
        public string FavoryName { get; set; }

        public M3UChannel() { }
        public M3UChannel(Models.M3UChannel c)
        {
            FullChannelString = c.FullChannelString;
            Name = c.Name;
            Url = c.Url;
            Group = c.Group;
            Logo = c.Logo;
        }
    }
}
