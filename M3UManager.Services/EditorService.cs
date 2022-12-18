using M3UManager.Models;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services
{
    public class EditorService : IEditorService
    {
        private M3UGroupsList[] groupsLists = new M3UGroupsList[2];
        private string[] selectedGroups;
        private List<M3UChannel> selectedChannels;
        private int activeGroupsList;
        private string[] commonGroups;

        public void AddGroupsList(M3UGroupsList group)
        {
            if (groupsLists[1] == null)
            {
                groupsLists[1] = group;
            }
            else
            {
                groupsLists[2] = group;
            }
        }

        public void RemoveGroupsList(int id)
        {
            groupsLists[id] = null;
        }

        public M3UGroupsList GetGroupsList(int id)
        {
            return groupsLists[id];
        }

        public string[] CompareGroupsLists()
        {
            var dict1Keys = groupsLists[0].M3UGroups.Keys;
            var dict2Keys = groupsLists[1].M3UGroups.Keys;
            commonGroups = dict1Keys.Where(x => dict2Keys.Contains(x)).ToArray();
            return commonGroups;
        }

        public string[] GetSelectedGroups()
        {
            return selectedGroups;
        }

        public void SetSelectedGroups(string[] groups)
        {
            selectedGroups = groups;
        }

        public List<M3UChannel> GetSelectedChannels()
        {
            return selectedChannels;
        }

        public void SetSelectedChannels(List<M3UChannel> channels)
        {
            selectedChannels = channels;
        }

        public void SetActiveGroupsList(int id)
        {
            activeGroupsList = id;
        }

        public int GetActiveGroupsList()
        {
            return activeGroupsList;
        }

        public bool IsGroupCommon(string channel)
        {
            return commonGroups.Contains(channel);
        }

    }
}