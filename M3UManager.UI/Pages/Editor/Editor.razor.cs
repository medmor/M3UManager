using M3UManager.Models.Commands;
using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;

namespace M3UManager.UI.Pages.Editor
{
    public partial class Editor
    {

        [Inject] IEditorService m3uService { get; set; }
        [Inject] ICommandFactory m3uCommandFactory { get; set; }
        public List<Command> Commands { get; set; } = new List<Command>();

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
        public void Refresh() => StateHasChanged();
        async Task OpenFile()
        {
            var cmd = m3uCommandFactory.GetCommand(CommandName.AddGroupsList);
            await cmd.Execute();
            Commands.Add(cmd);
        }

        public void Undo()
        {
            Commands[Commands.Count - 1].Undo();
            Commands.RemoveAt(Commands.Count - 1);
            StateHasChanged();
        }
        void CompareLists()
        {
            m3uService.CompareGroupsLists();
        }
        void CopyToOther(int modelId, int sourceModelId)
        {
            var cmd = m3uCommandFactory.GetCommand(CommandName.AddGroupsToList);
            cmd.Execute();
            Commands.Add(cmd);
        }
    }
}
