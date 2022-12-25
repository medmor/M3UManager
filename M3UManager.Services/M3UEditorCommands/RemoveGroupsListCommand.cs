using M3UManager.Services.M3UListServices;

namespace M3UManager.Services.M3UEditorCommands
{
    internal class RemoveGroupsListCommand : Models.Commands.Command
    {
        private readonly IEditorService M3UService;
        private int groupsListId { get; set; }
        private string m3uString { get; set; }


        public RemoveGroupsListCommand(IEditorService m3UService)
        {
            M3UService = m3UService;
            groupsListId = M3UService.GetActiveGroupsList();
        }

        public override Task Execute()
        {
            m3uString = M3UService.GetGroupsList(groupsListId).GetM3UString();
            M3UService.RemoveGroupsList(groupsListId);
            return Task.CompletedTask;
        }

        public override void Undo()
        {
            M3UService.AddGroupsList(new Models.M3UList(m3uString));
        }
    }
}
