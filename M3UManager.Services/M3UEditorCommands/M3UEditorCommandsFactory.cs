using M3UManager.Models.Commands;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    public class M3UEditorCommandsFactory : ICommandFactory
    {
        private readonly IEditorService m3UService;
        private readonly IFileIOService fileIOService;

        public M3UEditorCommandsFactory(IEditorService M3UService, IFileIOService file)
        {
            m3UService = M3UService;
            fileIOService = file;
        }

        public Models.Commands.Command GetCommand(CommandName command)
        {
            switch (command)
            {
                case CommandName.AddGroupsToList:
                    return new AddGroupsToListCommand(m3UService);
                case CommandName.RemoveGroupsList:
                    return new RemoveGroupsListCommand(m3UService);
                case CommandName.AddGroupsList:
                    return new AddGroupsListCommand(m3UService, fileIOService);
                case CommandName.RemoveGroupsFromList:
                    return new RemoveGroupsFromListCommand(m3UService);
                case CommandName.RemoveChannelsFromGroups:
                    return new RemoveChannelsFromGroupsCommand(m3UService);
                //case CommandName.AddChannelsToGroups:
                //    return new Add
                default: return null;
            }
        }
    }
}
