using M3UManager.Models;

namespace M3UManager.Services.ServicesContracts
{
    public interface IM3UService
    {
        public string[] SelectedGroups { get; set; }
        public M3UGroupList GetModel(int index);
        public string GetM3UString(int index);
        public int GroupListsCount();
        public M3UGroup[] GetGroupsFromModel(int modelId, string[] groupsNames);
        public void CompareGroupLists();
        public bool IsChannelInBothLists(string channelTrimmedName);
        public void AddGroupList(string m3uString);
        public void RemoveGroupList(int modelId);
        public void DeleteGroupsFromList(int modelId, string[] selected);
        public void AddGroupsToList(int modelId, M3UGroup[] groups);
        public void DeleteChannelsFromGroups(int modelId, List<M3UChannel> channels, string[] selectedGroups);
        public void AddChannelsToGroups(int modelId, List<M3UChannel> channels, string[] selectedGroups);
    }

}
