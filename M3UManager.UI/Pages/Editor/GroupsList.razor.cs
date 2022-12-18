﻿using M3UManager.Models;
using M3UManager.Models.Commands;
using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace M3UManager.UI.Pages.Editor
{
    public partial class GroupsList
    {
        [Inject] IEditorService m3UService { get; set; }
        [Inject] ICommandFactory commandFactory { get; set; }
        [Inject] IFileIOService fileIO { get; set; }
        [Inject] IJSRuntime js { get; set; }

        [Parameter] public int M3UListModelId { get; set; }
        [CascadingParameter] public Editor editor { get; set; }
        ChannelsList channelsList;
        Dictionary<string, M3UGroup> filtredGroups;
        string groupFilterString = "";

        protected override void OnInitialized()
        {
            filtredGroups = m3UService.GetGroupsList(M3UListModelId).M3UGroups;
        }

        void SaveList()
        {
            try
            {
                fileIO.SaveDictionaryAsM3U(m3UService.GetGroupsList(M3UListModelId).M3UGroups);
                js.InvokeVoidAsync("alert", "Save succeeded");
            }
            catch
            {
                js.InvokeVoidAsync("alert", "Save failed");
            }
        }
        async Task DeleteGroups()
        {
            var cmd = commandFactory.GetCommand(CommandName.RemoveGroupsFromList);
            cmd.Execute();
            editor.Commands.Add(cmd);
            FilterGroups(new ChangeEventArgs() { Value = groupFilterString });
            await js.InvokeVoidAsync("ChannelList.deselectItems");
            StateHasChanged();
        }
        void RemoveModel()
        {
            var cmd = commandFactory.GetCommand(CommandName.RemoveGroupsList);
            cmd.Execute();
            editor.Commands.Add(cmd);
            editor.Refresh();
        }
        void OnSelectGroupsInput(ChangeEventArgs args)
        {
            m3UService.SetSelectedGroups((string[])args.Value);
            List<M3UChannel> channels = new List<M3UChannel>();
            foreach (var key in (string[])args.Value)
            {
                channels = channels.Concat(m3UService.GetGroupsList(M3UListModelId).M3UGroups[key].Channels).ToList();
            }
            channelsList.OnGroupChanged(channels);
        }
        void FilterGroups(ChangeEventArgs args)
        {
            var model = m3UService.GetGroupsList(M3UListModelId);
            groupFilterString = (string)args.Value;
            if (string.IsNullOrEmpty(groupFilterString))
                filtredGroups = model.M3UGroups;
            else
            {
                filtredGroups = model.M3UGroups
                    .Where(g => g.Value.Name.Contains(groupFilterString, System.StringComparison.CurrentCultureIgnoreCase))
                    .ToDictionary(g => g.Key, g => g.Value);
            }
        }

        bool IsInBoothLists(string gTrimmedName) => m3UService.IsGroupCommon(gTrimmedName);
    }
}
