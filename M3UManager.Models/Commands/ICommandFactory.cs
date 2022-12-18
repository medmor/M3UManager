namespace M3UManager.Models.Commands
{
    public interface ICommandFactory
    {
        Command GetCommand(CommandName command);
    }
    public enum CommandName
    {
        AddGroupsList,
        RemoveGroupsList,
        AddGroupsToList,
        RemoveGroupsFromList,
        AddChannelsToGroups,
        RemoveChannelsFromGroups
    }
}
