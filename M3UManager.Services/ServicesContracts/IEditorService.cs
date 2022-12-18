using M3UManager.Models;

namespace M3UManager.Services.ServicesContracts
{
    public interface IEditorService
    {
        void AddGroupsList(M3UGroupsList group);
        void RemoveGroupsList(int id);
        M3UGroupsList GetGroupsList(int id);
        string[] CompareGroupsLists();
        bool IsGroupCommon(string channel);
        string[] GetSelectedGroups();
        void SetSelectedGroups(string[] groups);
        List<M3UChannel> GetSelectedChannels();
        void SetSelectedChannels(List<M3UChannel> channels);
        void SetActiveGroupsList(int id);
        int GetActiveGroupsList();

    }

}
