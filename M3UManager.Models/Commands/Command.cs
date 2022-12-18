namespace M3UManager.Models.Commands
{
    public abstract class Command
    {
        public abstract Task Execute();
        public abstract void Undo();
    }
}
