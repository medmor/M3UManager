using System.Text.RegularExpressions;

namespace M3UManager.Models
{
    public class Utils
    {
        public static string TrimmedString(string s) => Regex.Replace(s, @"\s", string.Empty);
    }
}
