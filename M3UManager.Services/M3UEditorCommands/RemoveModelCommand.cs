using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    internal class RemoveModelCommand : Models.Commands.Command
    {
        private readonly IM3UService M3UService;
        private int modelId { get; set; }
        private string m3uString { get; set; }


        public RemoveModelCommand(IM3UService m3UService, int modelId)
        {
            M3UService = m3UService;
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
