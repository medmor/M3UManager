using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    public class AddModelCommand : M3UEditorCommandBase
    {
        private int modelId;
        private string m3uString;

        public AddModelCommand(IM3UService m, int modelId, string m3uString)
            : base(m)
        {
            this.modelId = modelId;
            this.m3uString = m3uString;

        }
        public override void Execute()
        {
            M3UService.AddGroupList(m3uString);
        }

        public override void Undo()
        {
            M3UService.RemoveGroupList(modelId);
        }
    }
}
