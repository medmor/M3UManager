using M3UManager.Models.Commands;
using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    public class M3UEditorCommandsFactory : ICommandFactory
    {
        private readonly IM3UService M3UService;

        public M3UEditorCommandsFactory(IM3UService M3UService)
        {
            this.M3UService = M3UService;
        }

        public Models.Commands.Command GetCommand(int command)
        {
            switch (command)
            {
                case (int)CommandName.AddGroupToList:
                    return new AddGroupsToListCommand(M3UService);
            }
        }

        public enum CommandName
        {
            AddGroupToList,
            AddModel,
            DeleteChannelsFromGroups,
            DeleteGroupsFromList,
            RemveModel
        }
    }
}
