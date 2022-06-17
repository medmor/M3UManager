using M3UManager.Models;

namespace M3UManager.Services.ServicesContracts
{
    public interface IM3UService
    {
        public string[] SelectedGroups { get; set; }

        public void AddGroupList(string m3uString);
        public void RemoveGroupList(M3UGroupList model);
        public M3UGroupList GetModel(int index);
        public int GroupListsCount();
        public void CompareGroupLists();
        public void DeleteGroupsFromList(M3UGroupList model);
        public void AddGroupsToList(M3UGroupList model, M3UGroupList sourceModel);
        public bool IsInBothLists(string channelTrimmedName);
    }
}
