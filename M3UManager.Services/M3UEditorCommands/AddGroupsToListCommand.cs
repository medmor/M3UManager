using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    internal class AddGroupsToListCommand : Models.Commands.Command
    {
        private readonly IEditorService M3UService;
        private int groupsListId;
        private int sourceModelId;
        private string[] selected;
        public AddGroupsToListCommand(IEditorService m)
        {
            M3UService = m;
            groupsListId = m.GetActiveGroupsList();
            sourceModelId = groupsListId == 0 ? 1 : 0;
            selected = m.GetSelectedGroups();
        }

        public override Task Execute()
        {
            var groupsList = M3UService.GetGroupsList(groupsListId);
            var sourceGroupsList = M3UService.GetGroupsList(sourceModelId);

            var groups = sourceGroupsList.M3UGroups
                .Where(x => selected.Contains(x.Key))
                .Select(x => x.Value)
                .ToArray();

            groupsList.AddGroups(groups);
            return Task.CompletedTask;
        }

        public override void Undo()
        {
            var groupsList = M3UService.GetGroupsList(groupsListId);
            groupsList.RemoveGroups(selected);
        }
    }
}
