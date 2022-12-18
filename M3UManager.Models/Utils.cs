using System.Text.RegularExpressions;

namespace M3UManager.Models
{
    public class Utils
    {
        public static string Separator { get; private set; } = "#EXTINF:";
        public static Regex RegexGroupTitle { get; private set; } = new Regex("group-title=\"(.*)\"");
        public static Regex RegexChannelName { get; private set; } = new Regex("tvg-name=\"(.*?)\"");
        public static Regex RegexChannelLogo { get; private set; } = new Regex("tvg-logo=\"(.*?)\"");
        public static string TrimmedString(string s) => Regex.Replace(s, @"\s", string.Empty);
    }
}
