using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;

namespace M3UManager.UI.Pages.Editor
{
    public partial class Editor
    {

        [Inject] IM3UService m3uService { get; set; }
        [Inject] IFileIOService fileIO { get; set; }
        public List<Models.Commands.Command> Commands { get; set; } = new List<Models.Commands.Command>();

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
        public void Refresh() => StateHasChanged();
        async Task OpenFile()
        {
            var textFile = await fileIO.OpenM3U();
            if (!string.IsNullOrEmpty(textFile))
            {
                var cmd = new Services.M3UEditorCommands.AddModelCommand(m3uService, m3uService.GroupListsCount(), textFile);
                cmd.Execute();
                Commands.Add(cmd);
            }
        }

        public void Undo()
        {
            Commands[Commands.Count - 1].Undo();
            Commands.RemoveAt(Commands.Count - 1);
        }
        void CompareLists()
        {
            m3uService.CompareGroupLists();
        }
        void CopyToOther(int modelId, int sourceModelId) => m3uService.AddGroupsToList(modelId, sourceModelId, m3uService.SelectedGroups);
    }
}
