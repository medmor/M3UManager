namespace M3UManager.Models.Commands
{
    public interface ICommandFactory
    {
        Command GetCommand(int command);
    }
}
