using M3UManager.Models;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    internal class RemoveChannelsFromGroupsCommand : Models.Commands.Command
    {
        private readonly IEditorService M3UService;
        private int groupsListId;
        private List<M3UChannel> channels;
        private string[] selectedGroups;
        public RemoveChannelsFromGroupsCommand(IEditorService m3UService)
        {
            M3UService = m3UService;
            groupsListId = m3UService.GetActiveGroupsList();
            channels = m3UService.GetSelectedChannels();
            selectedGroups = m3UService.GetSelectedGroups();
        }

        public override void Execute()
        {
            var groupList = M3UService.GetGroupsList(groupsListId);
            foreach (var group in selectedGroups)
            {
                var g = groupList.GetGroup(group);
                g.RemoveChannels(channels.Select(c => c.Name).ToList());
            }
        }

        public override void Undo()
        {
            var groupList = M3UService.GetGroupsList(groupsListId);
            foreach (var group in selectedGroups)
            {
                M3UGroup g = groupList.GetGroup(group);
                g.AddChannels(channels);
            }
        }
    }
}
