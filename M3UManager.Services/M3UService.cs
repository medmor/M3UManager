using M3UManager.Models;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services
{
    public class M3UService : IM3UService
    {
        public string[] SelectedGroups { get; set; } = new string[0];
        private readonly List<M3UGroupList> groupLists = new List<M3UGroupList>();
        private string[] inBothLists = new string[0];

        public M3UGroupList GetModel(int index) => groupLists[index];
        public string GetM3UString(int index) => GetModel(index).GetM3UString();
        public M3UGroup[] GetGroupsFromModel(int modelId, string[] groupsNames) => GetModel(modelId).M3UGroups
                .Where(x => groupsNames.Contains(x.Key))
                .Select(x => x.Value)
                .ToArray();
        public void CompareGroupLists()
        {
            var dict1Keys = GetModel(0).M3UGroups.Keys;
            var dict2Keys = GetModel(1).M3UGroups.Keys;

            inBothLists = dict1Keys.Where(x => dict2Keys.Contains(x)).ToArray();
        }
        public bool IsChannelInBothLists(string channelTrimmedName) => groupLists.Count > 1 && inBothLists.Contains(channelTrimmedName);
        public void AddGroupList(string m3uString)
        {
            if (groupLists.Count < 2)
            {
                groupLists.Add(new M3UGroupList(m3uString));
            }
        }
        public void RemoveGroupList(int modelId) => groupLists.Remove(GetModel(modelId));
        public int GroupListsCount() => groupLists.Count();
        public void DeleteGroupsFromList(int modelId, string[] selected)
        {
            if (selected.Length > 0)
                GetModel(modelId).RemoveGroups(SelectedGroups);
        }
        public void AddGroupsToList(int modelId, M3UGroup[] groups)
        {
            GetModel(modelId).AddGroups(groups);
        }
        public void DeleteChannelsFromGroups(int modelId, List<M3UChannel> channels, string[] selectedGroups)
        {
            foreach (var group in selectedGroups)
            {
                var g = GetModel(modelId).GetGroup(group);
                g.RemoveChannels(channels.Select(c => c.Name).ToList());
                //g.Channels = g.Channels.Where(c => !channels.Contains(c)).ToList();
            }
        }

        public void AddChannelsToGroups(int modelId, List<M3UChannel> channels, string[] selectedGroups)
        {
            foreach (var group in selectedGroups)
            {
                M3UGroup g = GetModel(modelId).GetGroup(group);
                g.AddChannels(channels);
            }
        }
    }
}
