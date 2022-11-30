using M3UManager.Models;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    public class DeleteChannelsFromGroupsCommand : M3UEditorCommandBase
    {
        private int modelId;
        private List<M3UChannel> channels;
        private string[] selectedGroups;
        public DeleteChannelsFromGroupsCommand(IM3UService m3UService, int id, List<M3UChannel> ch, string[] selectedGroups)
            : base(m3UService)
        {
            modelId = id;
            channels = ch;
            this.selectedGroups = selectedGroups;
        }

        public override void Execute()
        {
            M3UService.DeleteChannelsFromGroups(modelId, channels, selectedGroups);
        }

        public override void Undo()
        {
            M3UService.AddChannelsToGroups(modelId, channels, selectedGroups);
        }
    }
}
