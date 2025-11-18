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
            var model = m3UService.GetModel(M3UListModelId);
            filtredGroups = model.M3UGroups
                .Where(g => IsGroupMatchingContentFilter(g.Value))
                .OrderByDescending(g => g.Value.Channels.Count)
                .ToDictionary(g => g.Key, g => g.Value);
        }

        private bool IsGroupMatchingContentFilter(M3UGroup group)
        {
            // If no filter is set, show all groups
            if (string.IsNullOrEmpty(editor.ContentTypeFilter))
                return true;

            // Check if group name starts with the content type prefix
            var prefix = editor.ContentTypeFilter switch
            {
                "Movie" => "🎬 Movies",
                "TV Show" => "📺 TV Shows",
                "Live TV" => "📺 Live TV",
                _ => ""
            };

            return group.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
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
            var channels = new List<M3UChannel>();
            foreach (var key in selected)
            {
                channels.AddRange(m3UService.GetModel(M3UListModelId).M3UGroups[key].Channels);
            }
            channelsList.OnGroupChanged(channels);
        }
        void FilterGroups(ChangeEventArgs args)
        {
            var model = m3UService.GetModel(M3UListModelId);
            groupFilterString = args.Value?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(groupFilterString))
            {
                filtredGroups = model.M3UGroups
                    .Where(g => IsGroupMatchingContentFilter(g.Value))
                    .OrderByDescending(g => g.Value.Channels.Count)
                    .ToDictionary(g => g.Key, g => g.Value);
            }
            else
            {
                filtredGroups = model.M3UGroups
                    .Where(g => IsGroupMatchingContentFilter(g.Value) && 
                               g.Value.Name.Contains(groupFilterString, System.StringComparison.CurrentCultureIgnoreCase))
                    .OrderByDescending(g => g.Value.Channels.Count)
                    .ToDictionary(g => g.Key, g => g.Value);
            }
        }

        bool IsInBoothLists(string gTrimmedName) => m3UService.IsChannelInBothLists(gTrimmedName);
    }
}
