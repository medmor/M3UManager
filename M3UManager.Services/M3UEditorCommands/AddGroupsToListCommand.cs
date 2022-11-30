using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    public class AddGroupsToListCommand : M3UEditorCommandBase
    {
        private int modelId;
        private int sourceModelId;
        private string[] selected;
        public AddGroupsToListCommand(IM3UService m, int modelId, int sourceModelId, string[] selected)
            : base(m)
        {
            this.modelId = modelId;
            this.sourceModelId = sourceModelId;
            this.selected = selected;
        }

        public override void Execute()
        {
            M3UService.AddGroupsToList(modelId, M3UService.GetGroupsFromModel(sourceModelId, selected));
        }

        public override void Undo()
        {
            M3UService.DeleteGroupsFromList(modelId, selected);
        }
    }
}
