using M3UManager.Services.FileIOServices;
using M3UManager.Services.M3UListServices;

namespace M3UManager.Services.M3UEditorCommands
{
    internal class AddGroupsListCommand : Models.Commands.Command
    {
        private readonly IEditorService M3UService;
        private readonly IFileIOService fileIOService;

        private int groupsListId;
        private string m3uString;

        public AddGroupsListCommand(IEditorService m, IFileIOService file)
        {
            M3UService = m;
            fileIOService = file;
            groupsListId = m.GetActiveGroupsList();
        }
        public override async Task Execute()
        {
            var textFile = await fileIOService.OpenM3U();
            if (!string.IsNullOrEmpty(textFile))
            {
                m3uString = textFile;
                M3UService.AddGroupsList(new Models.M3UList(m3uString));
            }
        }

        public override void Undo()
        {
            M3UService.RemoveGroupsList(groupsListId);
        }
    }
}
