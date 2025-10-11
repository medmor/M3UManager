using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.IO;
using System;

namespace M3UManager.UI.Pages.Editor
{
    public partial class GroupsList
    {
        [Inject] IM3UService m3UService { get; set; } = default!;
        [Inject] IFileIOService fileIO { get; set; } = default!;
        [Inject] IJSRuntime js { get; set; } = default!;

        [Parameter] public int M3UListModelId { get; set; }
        [CascadingParameter] public Editor editor { get; set; } = default!;
        ChannelsList channelsList = default!;
        Dictionary<string, M3UGroup> filtredGroups = default!;
        string groupFilterString = "";

        protected override void OnInitialized()
        {
            filtredGroups = m3UService.GetModel(M3UListModelId).M3UGroups;
        }

        async Task SaveList()
        {
            try
            {
                var groups = m3UService.GetModel(M3UListModelId).M3UGroups;
                var downloadsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                var savePath = Path.Combine(downloadsDir, "channels.m3u");
                await fileIO.SaveDictionaryAsM3U(groups, downloadsDir);
                await js.InvokeVoidAsync("alert", $"Save succeeded: {savePath}");
            }
            catch (Exception ex)
            {
                await js.InvokeVoidAsync("alert", $"Save failed: {ex.Message}");
            }
        }
        async Task DeleteGroups()
        {
            var cmd = new Services.M3UEditorCommands.DeleteGroupsFromListCommand(m3UService, M3UListModelId, m3UService.SelectedGroups);
            cmd.Execute();
            editor.Commands.Add(cmd);
            FilterGroups(new ChangeEventArgs() { Value = groupFilterString });
            await js.InvokeVoidAsync("ChannelList.deselectItems");
            StateHasChanged();
        }
        void RemoveModel()
        {
            var cmd = new Services.M3UEditorCommands.RemoveModelCommand(m3UService, M3UListModelId);
            cmd.Execute();
            editor.Commands.Add(cmd);
            editor.Refresh();
        }
        void OnSelectGroupsInput(ChangeEventArgs args)
        {
            var selected = args.Value as string[];
            if (selected is null || selected.Length == 0)
            {
                m3UService.SelectedGroups = Array.Empty<string>();
                channelsList.OnGroupChanged(new List<M3UChannel>());
                return;
            }
            m3UService.SelectedGroups = selected;
            List<M3UChannel> channels = new List<M3UChannel>();
            foreach (var key in selected)
            {
                channels = channels.Concat(m3UService.GetModel(M3UListModelId).M3UGroups[key].Channels).ToList();
            }
            channelsList.OnGroupChanged(channels);
        }
        void FilterGroups(ChangeEventArgs args)
        {
            var model = m3UService.GetModel(M3UListModelId);
            groupFilterString = args.Value?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(groupFilterString))
                filtredGroups = model.M3UGroups;
            else
            {
                filtredGroups = model.M3UGroups
                    .Where(g => g.Value.Name.Contains(groupFilterString, System.StringComparison.CurrentCultureIgnoreCase))
                    .ToDictionary(g => g.Key, g => g.Value);
            }
        }

        bool IsInBoothLists(string gTrimmedName) => m3UService.IsChannelInBothLists(gTrimmedName);
    }
}
