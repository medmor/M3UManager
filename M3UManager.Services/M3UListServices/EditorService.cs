﻿using M3UManager.Models;

namespace M3UManager.Services.M3UListServices
{
    public class EditorService : IEditorService
    {
        private M3UList[] groupsLists = new M3UList[2];
        private string[] selectedGroups;
        private List<M3UChannel> selectedChannels;
        private int activeGroupsList;
        private string[] commonGroups;

        public void AddGroupsList(M3UList group)
        {
            if (groupsLists[0] == null)
            {
                groupsLists[0] = group;
            }
            else
            {
                groupsLists[1] = group;
            }
        }

        public void RemoveGroupsList(int id)
        {
            groupsLists[id] = null;
        }

        public M3UList GetGroupsList(int id)
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
            if (commonGroups == null)
                return false;
            return commonGroups.Contains(channel);
        }

    }
}