namespace M3UManager.Models
{
    public class Favory
    {
        public string Name { get; set; }
        public ICollection<M3UChannel> Channels { get; set; }

        public Favory(string name = "untagged")
        {
            Channels = new List<M3UChannel>();
            Name = name;
        }
    }
}
