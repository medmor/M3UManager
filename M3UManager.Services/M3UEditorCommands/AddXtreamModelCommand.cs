using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    public class AddXtreamModelCommand : M3UEditorCommandBase
    {
        private int modelId;
        private string xtreamUrl;

        public AddXtreamModelCommand(IM3UService m, int modelId, string xtreamUrl)
            : base(m)
        {
            this.modelId = modelId;
            this.xtreamUrl = xtreamUrl;
        }

        public override void Execute()
        {
            // Execute is not used since we call the service directly in the UI
            // This is kept for undo functionality
        }

        public override void Undo()
        {
            M3UService.RemoveGroupList(modelId);
        }
    }
}
