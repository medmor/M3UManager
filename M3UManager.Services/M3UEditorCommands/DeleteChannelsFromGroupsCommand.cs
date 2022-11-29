using M3UManager.Models;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    internal class DeleteChannelsFromGroupsCommand : M3UEditorCommandBase
    {
        private int modelId;
        private List<M3UChannel> channels;
        public DeleteChannelsFromGroupsCommand(IM3UService m3UService)
            : base(m3UService)
        {
        }

        public override void Execute()
        {
            M3UService.DeleteChannelsFromGroups(modelId, channels);
        }

        public override void Undo()
        {
            M3UService.AddChannelsToGroups(modelId, channels);
        }
    }
}
