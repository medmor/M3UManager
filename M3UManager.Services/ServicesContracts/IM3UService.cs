using M3UManager.Models;

namespace M3UManager.Services.ServicesContracts
{
    public interface IM3UService
    {
        public string[] SelectedGroups { get; set; }
        public M3UGroupList GetModel(int index);
        public string GetM3UString(int index);
        public int GroupListsCount();
        public void CompareGroupLists();
        public bool IsInBothLists(string channelTrimmedName);
        public void AddGroupList(string m3uString);
        public void RemoveGroupList(int modelId);
        public void DeleteGroupsFromList(int modelId, string[] selected);
        public void AddGroupsToList(int modelId, int sourceModelId, string[] selected);
        public void DeleteChannelsFromGroups(int modelId, List<M3UChannel> channels);
        public void AddChannelsToGroups(int modelId, List<M3UChannel> channels);
    }

}
