using M3UManager.Models;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    public class DeleteGroupsFromListCommand : M3UEditorCommandBase
    {
        private int modelId;
        private string[] selected;
        private M3UGroup[] deletedGroups;
        public DeleteGroupsFromListCommand(IM3UService m3UService, int modelId, string[] selected)
            : base(m3UService)
        {
            this.modelId = modelId;
            this.selected = selected;
        }

        public override void Execute()
        {
            deletedGroups = M3UService.GetGroupsFromModel(modelId, selected);
            M3UService.DeleteGroupsFromList(modelId, selected);
        }

        public override void Undo()
        {
            M3UService.AddGroupsToList(modelId, deletedGroups);
        }
    }
}
