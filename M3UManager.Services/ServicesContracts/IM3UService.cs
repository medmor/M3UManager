using M3UManager.Models;

namespace M3UManager.Services.ServicesContracts
{
    public interface IM3UService
    {
        public string[] SelectedGroups { get; set; }

        public void AddGroupList(string m3uString);
        public void RemoveGroupList(int modelId);
        public M3UGroupList GetModel(int index);
        public int GroupListsCount();
        public void CompareGroupLists();
        public void DeleteGroupsFromList(int model);
        public void DeleteChannelsFromGroups(int modelId, List<M3UChannel> channels);
        public void AddGroupsToList(M3UGroupList model, M3UGroupList sourceModel);
        public bool IsInBothLists(string channelTrimmedName);
    }
}
