﻿using M3UManager.Models;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services
{
    public class M3UService : IM3UService
    {
        public string[] SelectedGroups { get; set; } = new string[0];
        private readonly List<M3UGroupList> groupLists = new List<M3UGroupList>();
        private string[] inBothLists = new string[0];

        public void AddGroupList(string m3uString)
        {
            if (groupLists.Count < 2)
            {
                groupLists.Add(new M3UGroupList(m3uString));
            }
        }
        public void RemoveGroupList(M3UGroupList model) => groupLists.Remove(model);
        public M3UGroupList GetModel(int index) => groupLists[index];
        public int GroupListsCount() => groupLists.Count;
        public void CompareGroupLists()
        {
            var dict1Keys = GetModel(0).M3UGroups.Keys;
            var dict2Keys = GetModel(1).M3UGroups.Keys;

            inBothLists = dict1Keys.Where(x => dict2Keys.Contains(x)).ToArray();
        }
        public void DeleteGroupsFromList(M3UGroupList model)
        {
            if (SelectedGroups.Length > 0)
                model.RemoveGroups(SelectedGroups);
        }
        public void AddGroupsToList(M3UGroupList model, M3UGroupList sourceModel)
        {
            var list = sourceModel.M3UGroups
                .Where(x => SelectedGroups.Contains(x.Key))
                .Select(x => x.Value)
                .ToArray();
            model.AddGroups(list);
        }
        public bool IsInBothLists(string channelTrimmedName) => groupLists.Count > 1 && inBothLists.Contains(channelTrimmedName);

    }
}
