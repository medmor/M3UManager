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
        public void AddGroupList(string m3uString)
        {
            if (groupLists.Count < 2)
            {
                groupLists.Add(new M3UGroupList(m3uString));
            }
        }
        public void RemoveGroupList(int modelId) => groupLists.Remove(GetModel(modelId));
        public int GroupListsCount() => groupLists.Count();
        public void CompareGroupLists()
        {
            var dict1Keys = GetModel(0).M3UGroups.Keys;
            var dict2Keys = GetModel(1).M3UGroups.Keys;

            inBothLists = dict1Keys.Where(x => dict2Keys.Contains(x)).ToArray();
        }
        public void DeleteGroupsFromList(int modelId, string[] selected)
        {
            if (selected.Length > 0)
                GetModel(modelId).RemoveGroups(SelectedGroups);
        }
        public void DeleteChannelsFromGroups(int modelId, List<M3UChannel> channels)
        {
            foreach (var group in SelectedGroups)
            {
                var g = GetModel(modelId).GetGroup(group);
                g.Channels = g.Channels.Where(c => !channels.Contains(c)).ToList();
            }
        }
        public void AddGroupsToList(int modelId, int sourceModelId, string[] selected)
        {
            var list = GetModel(sourceModelId).M3UGroups
                .Where(x => selected.Contains(x.Key))
                .Select(x => x.Value)
                .ToArray();
            GetModel(modelId).AddGroups(list);
        }
        public bool IsInBothLists(string channelTrimmedName) => groupLists.Count > 1 && inBothLists.Contains(channelTrimmedName);

    }
}
