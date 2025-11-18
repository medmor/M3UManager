using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;

namespace M3UManager.UI.Pages.Editor
{
    public partial class Editor
    {

        [Inject] IM3UService m3uService { get; set; }
        [Inject] IFileIOService fileIO { get; set; }
        public List<Models.Commands.Command> Commands { get; set; } = new List<Models.Commands.Command>();
        
        private bool showXtreamDialog = false;
        private string xtreamUrl = string.Empty;
        private bool isLoading = false;
        private string errorMessage = string.Empty;

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

        void ShowXtreamDialog()
        {
            showXtreamDialog = true;
            xtreamUrl = string.Empty;
            errorMessage = string.Empty;
            StateHasChanged();
        }

        void CloseXtreamDialog()
        {
            showXtreamDialog = false;
            xtreamUrl = string.Empty;
            errorMessage = string.Empty;
            isLoading = false;
            StateHasChanged();
        }

        async Task LoadFromXtream()
        {
            if (string.IsNullOrWhiteSpace(xtreamUrl))
                return;

            isLoading = true;
            errorMessage = string.Empty;
            StateHasChanged();

            try
            {
                var currentCount = m3uService.GroupListsCount();
                await m3uService.AddGroupListFromXtreamAsync(xtreamUrl);
                
                var cmd = new Services.M3UEditorCommands.AddXtreamModelCommand(m3uService, currentCount, xtreamUrl);
                Commands.Add(cmd);
                
                CloseXtreamDialog();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to load from Xtream: {ex.Message}";
                isLoading = false;
                StateHasChanged();
            }
        }

        public void Undo()
        {
            Commands[Commands.Count - 1].Undo();
            Commands.RemoveAt(Commands.Count - 1);
            StateHasChanged();
        }
        void CompareLists()
        {
            m3uService.CompareGroupLists();
        }
        void CopyToOther(int modelId, int sourceModelId)
            => m3uService.AddGroupsToList(modelId, m3uService.GetGroupsFromModel(sourceModelId, m3uService.SelectedGroups));
    }
}
