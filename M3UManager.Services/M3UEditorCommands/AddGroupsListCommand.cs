using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    internal class AddGroupsListCommand : Models.Commands.Command
    {
        private readonly IEditorService M3UService;
        private int groupsListId;
        private string m3uString;

        public AddGroupsListCommand(IEditorService m)
        {
            M3UService = m;
            groupsListId = m.GetActiveGroupsList();
            m3uString = M3UService.GetGroupsList(groupsListId).GetM3UString();

        }
        public override void Execute()
        {
            M3UService.AddGroupsList(new Models.M3UGroupsList(m3uString));
        }

        public override void Undo()
        {
            M3UService.RemoveGroupsList(groupsListId);
        }
    }
}
