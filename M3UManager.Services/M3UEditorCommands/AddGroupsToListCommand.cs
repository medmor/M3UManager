using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    internal class AddGroupsToListCommand : Models.Commands.Command
    {
        private readonly IM3UService M3UService;
        private int modelId;
        private int sourceModelId;
        private string[] selected;
        public AddGroupsToListCommand(IM3UService m, int modelId, int sourceModelId, string[] selected)
        {
            M3UService = m;
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
