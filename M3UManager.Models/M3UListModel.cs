using System.Text.RegularExpressions;

namespace M3UManager.Models
{
    public class M3UGroupList
    {
    private readonly string separator = "#EXTINF:";
    private readonly Regex regex = new Regex("group-title=\"(.*?)\"", RegexOptions.Compiled);

        public Dictionary<string, M3UGroup> M3UGroups { get; set; }
        public M3UGroupList() { }
        public M3UGroupList(string m3uListString)
        {
            var lines = m3uListString.Split(separator);
            // Filter segments that actually have a group-title and normalize the key once to avoid collisions
            var groups = lines
                .Where(l => regex.IsMatch(l))
                .GroupBy(l => Utils.TrimmedString(regex.Match(l).Groups[1].Value), StringComparer.OrdinalIgnoreCase);

            var groupsDict = groups.ToDictionary(
                group => group.Key,
                group =>
                {
                    // Use the first segment's original group-title for display
                    var originalName = regex.Match(group.First()).Groups[1].Value;
                    var channels = group.Select(c => new M3UChannel(c, originalName)).ToList();
                    return new M3UGroup(originalName, channels);
                },
                StringComparer.OrdinalIgnoreCase);

            // Sort groups by descending channel count
            M3UGroups = groupsDict
                .OrderByDescending(g => g.Value.Channels.Count)
                .ToDictionary(g => g.Key, g => g.Value, StringComparer.OrdinalIgnoreCase);
        }

        public M3UGroup GetGroup(string groupName) => M3UGroups[groupName];
        public string GetM3UString()
        {
            var list = M3UGroups.Values.SelectMany(d => d.Channels).ToArray();
            return string.Join(separator, list.Select(c => c.FullChannelString));
        }
        public void AddGroup(M3UGroup group)
        {
            if (M3UGroups.ContainsKey(group.Name))
                return;
            M3UGroups.Add(group.TrimedName, group);
        }
        public void AddGroups(M3UGroup[] groups)
        {
            foreach (var group in groups)
                AddGroup(group);
        }
        public void RemoveGroup(string key) => M3UGroups.Remove(key);
        public void RemoveGroups(string[] keys)
        {
            foreach (var key in keys)
                RemoveGroup(key);
        }

        public static async Task<M3UGroupList> FromFileAsync(string path, CancellationToken cancellationToken = default)
        {
            var list = new M3UGroupList { M3UGroups = new Dictionary<string, M3UGroup>() };

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 64, useAsync: true);
            using var reader = new StreamReader(stream);

            string line;
            string currentExtInf = null;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (line.StartsWith("#EXTINF:"))
                {
                    currentExtInf = line;
                    continue;
                }

                if (currentExtInf != null)
                {
                    // next non-comment line is expected to be the URL
                    if (line.Length == 0 || line[0] == '#')
                    {
                        // skip comment/empty between EXTINF and URL just in case
                        continue;
                    }

                    var fullChannel = currentExtInf + "\n" + line;
                    var grp = Regex.Match(currentExtInf, "group-title=\"(.*?)\"").Groups[1].Value;
                    var trimmed = Utils.TrimmedString(grp);
                    if (!list.M3UGroups.TryGetValue(trimmed, out var group))
                    {
                        group = new M3UGroup(grp, new List<M3UChannel>());
                        list.M3UGroups[trimmed] = group;
                    }
                    group.AddChannel(new M3UChannel(fullChannel, grp));
                    currentExtInf = null;
                }
            }

            // Sort groups by descending channel count
            list.M3UGroups = list.M3UGroups
                .OrderByDescending(g => g.Value.Channels.Count)
                .ToDictionary(g => g.Key, g => g.Value, StringComparer.OrdinalIgnoreCase);

            return list;
        }
    }
}
