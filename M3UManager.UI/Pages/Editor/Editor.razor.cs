using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;

namespace M3UManager.UI.Pages.Editor
{
    public partial class Editor
    {

        [Inject] IM3UService m3uService { get; set; }
        [Inject] IFileIOService fileIO { get; set; }

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
                m3uService.AddGroupList(textFile);
            }
        }
        bool isOdd(int num) => !(num % 2 == 0);
        void CompareLists()
        {
            m3uService.CompareGroupLists();
        }
        void CopyToOther(M3UGroupList model, M3UGroupList sourceModel) => m3uService.AddGroupsToList(model, sourceModel);
    }
}
