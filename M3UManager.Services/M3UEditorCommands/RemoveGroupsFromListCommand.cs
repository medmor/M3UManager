using M3UManager.Models;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    internal class RemoveGroupsFromListCommand : Models.Commands.Command
    {
        private readonly IEditorService M3UService;
        private int groupsListId;
        private string[] selected;
        private M3UGroup[] deletedGroups;
        public RemoveGroupsFromListCommand(IEditorService m3UService)
        {
            M3UService = m3UService;
            groupsListId = M3UService.GetActiveGroupsList();
            selected = M3UService.GetSelectedGroups();
        }

        public override Task Execute()
        {
            var groupsList = M3UService.GetGroupsList(groupsListId);
            deletedGroups = groupsList.M3UGroups
                .Where(x => selected.Contains(x.Key))
                .Select(x => x.Value)
                .ToArray();
            groupsList.RemoveGroups(selected);
            return Task.CompletedTask;
        }

        public override void Undo()
        {
            var groupsList = M3UService.GetGroupsList(groupsListId);
            groupsList.AddGroups(deletedGroups);
        }
    }
}
