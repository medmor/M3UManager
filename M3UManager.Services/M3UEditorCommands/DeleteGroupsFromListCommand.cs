using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    public class DeleteGroupsFromListCommand : M3UEditorCommandBase
    {
        private int modelId;
        private string[] selected;
        public DeleteGroupsFromListCommand(IM3UService m3UService, int modelId, string[] selected)
            : base(m3UService)
        {
            this.modelId = modelId;
            this.selected = selected;
        }

        public override void Execute()
        {
            M3UService.DeleteGroupsFromList(modelId, selected);
        }

        public override void Undo()
        {
            M3UService.AddGroupsToList(modelId, modelId, selected);
        }
    }
}
