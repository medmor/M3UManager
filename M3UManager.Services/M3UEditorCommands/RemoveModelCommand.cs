using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    public class RemoveModelCommand : M3UEditorCommandBase
    {
        private int modelId { get; set; }
        private string m3uString { get; set; }


        public RemoveModelCommand(IM3UService m3UService, int modelId)
            : base(m3UService)
        {
            this.modelId = modelId;
        }

        public override void Execute()
        {
            m3uString = M3UService.GetM3UString(modelId);
            M3UService.RemoveGroupList(modelId);
        }

        public override void Undo()
        {
            M3UService.AddGroupList(m3uString);
        }
    }
}
