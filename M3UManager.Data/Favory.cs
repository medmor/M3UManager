using System.ComponentModel.DataAnnotations;

namespace M3UManager.Data
{
    public class Favory
    {
        [Key, Required]
        public string Name { get; set; }
        public ICollection<M3UChannel> Channels { get; set; }

        public Favory(string name = "untagged")
        {
            Channels = new List<M3UChannel>();
            Name = name;
        }
    }
}
