using M3UManager.Services.ServicesContracts;

namespace M3UManager.Services.M3UEditorCommands
{
    public abstract class M3UEditorCommandBase : Models.Commands.Command
    {
        internal IM3UService M3UService { get; set; }
        public M3UEditorCommandBase(IM3UService m3UService)
        {
            M3UService = m3UService;
        }
    }
}
