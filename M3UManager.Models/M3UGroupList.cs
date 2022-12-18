namespace M3UManager.Models
{
    public class M3UGroupsList
    {
        public Dictionary<string, M3UGroup> M3UGroups { get; set; }
        public M3UGroupsList() { }
        public M3UGroupsList(string m3uListString)
        {
            var lines = m3uListString.Split(Utils.Separator);
            M3UGroups = lines.GroupBy(l => Utils.RegexGroupTitle.Match(l).Groups[1].Value)
                    .ToDictionary(group => Utils.TrimmedString(group.Key), group => new M3UGroup(group.Key, group));
        }

        public M3UGroup GetGroup(string groupName) => M3UGroups[groupName];
        public string GetM3UString()
        {
            var list = M3UGroups.Values.SelectMany(d => d.Channels).ToArray();
            return string.Join(Utils.Separator, list.Select(c => c.FullChannelString));
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
    }
}
