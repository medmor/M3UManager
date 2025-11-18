using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using M3UManager.Models.XtreamModels;

namespace M3UManager.Models
{
    public class M3UGroupList
    {
        [JsonIgnore]
        private readonly string separator = "#EXTINF:";
        [JsonIgnore]
        private readonly Regex regex = new Regex("group-title=\"(.*?)\"", RegexOptions.Compiled);

        public Dictionary<string, M3UGroup> M3UGroups { get; set; }
        
        public M3UGroupList() 
        {
            M3UGroups = new Dictionary<string, M3UGroup>(StringComparer.OrdinalIgnoreCase);
        }
        
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

        // Factory method to create M3UGroupList from Xtream Codes API data
        public static M3UGroupList FromXtreamData(
            List<XtreamCategory> liveCategories,
            List<XtreamChannel> liveChannels,
            List<XtreamCategory> vodCategories,
            List<XtreamChannel> vodStreams,
            List<XtreamCategory> seriesCategories,
            List<XtreamChannel> seriesStreams,
            string serverUrl,
            string username,
            string password)
        {
            var list = new M3UGroupList { M3UGroups = new Dictionary<string, M3UGroup>(StringComparer.OrdinalIgnoreCase) };

            // Process Live TV
            ProcessXtreamContent(list, liveCategories, liveChannels, serverUrl, username, password, 
                ContentType.LiveTV, "📺 Live TV", 
                (streamId) => $"{serverUrl}/live/{username}/{password}/{streamId}.m3u8");

            // Process Movies (VOD)
            ProcessXtreamContent(list, vodCategories, vodStreams, serverUrl, username, password, 
                ContentType.Movie, "🎬 Movies", 
                (streamId) => $"{serverUrl}/movie/{username}/{password}/{streamId}.mp4");

            // Process Series
            ProcessXtreamContent(list, seriesCategories, seriesStreams, serverUrl, username, password, 
                ContentType.Series, "📺 TV Shows", 
                (streamId) => $"{serverUrl}/series/{username}/{password}/{streamId}.m3u8");

            // Sort groups by descending channel count
            list.M3UGroups = list.M3UGroups
                .OrderByDescending(g => g.Value.Channels.Count)
                .ToDictionary(g => g.Key, g => g.Value, StringComparer.OrdinalIgnoreCase);

            return list;
        }

        private static void ProcessXtreamContent(
            M3UGroupList list,
            List<XtreamCategory> categories,
            List<XtreamChannel> channels,
            string serverUrl,
            string username,
            string password,
            ContentType contentType,
            string prefix,
            Func<int, string> urlBuilder)
        {
            if (channels == null || channels.Count == 0)
                return;

            // Create a dictionary of categories for quick lookup
            var categoryDict = categories?.ToDictionary(c => c.CategoryId, c => c.CategoryName) 
                ?? new Dictionary<string, string>();

            // Group channels by category
            var groupedChannels = channels.GroupBy(c => c.CategoryId);

            foreach (var group in groupedChannels)
            {
                // Get category name, or use "Uncategorized" if not found
                var categoryName = categoryDict.TryGetValue(group.Key, out var catName) 
                    ? catName 
                    : "Uncategorized";
                
                // Prefix category with content type
                var fullCategoryName = $"{prefix} - {categoryName}";
                var trimmedKey = Utils.TrimmedString(fullCategoryName);

                // Create M3UChannel objects from XtreamChannel objects
                var m3uChannels = group.Select(xChannel =>
                {
                    var streamUrl = urlBuilder(xChannel.StreamId);
                    return new M3UChannel(xChannel, fullCategoryName, streamUrl, contentType, serverUrl, username, password);
                }).ToList();

                var m3uGroup = new M3UGroup(fullCategoryName, m3uChannels);
                list.M3UGroups[trimmedKey] = m3uGroup;
            }
        }
    }
}
