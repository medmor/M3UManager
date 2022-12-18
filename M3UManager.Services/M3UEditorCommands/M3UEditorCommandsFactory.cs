using M3UManager.Models.Commands;

namespace M3UManager.Services.M3UEditorCommands
{
    public class M3UEditorCommandsFactory : ICommandFactory
    {
        private readonly ServicesContracts.IEditorService M3UService;

        public M3UEditorCommandsFactory(ServicesContracts.IEditorService M3UService)
        {
            this.M3UService = M3UService;
        }

        public Models.Commands.Command GetCommand(CommandName command)
        {
            switch (command)
            {
                case CommandName.AddGroupsToList:
                    return new AddGroupsToListCommand(M3UService);
                case CommandName.RemoveGroupsList:
                    return new RemoveGroupsListCommand(M3UService);
                case CommandName.AddGroupsList:
                    return new AddGroupsListCommand(M3UService);
                case CommandName.RemoveGroupsFromList:
                    return new RemoveGroupsFromListCommand(M3UService);
                case CommandName.RemoveChannelsFromGroups:
                    return new RemoveChannelsFromGroupsCommand(M3UService);
                //case CommandName.AddChannelsToGroups:
                //    return new Add
                default: return null;
            }
        }
    }
}
