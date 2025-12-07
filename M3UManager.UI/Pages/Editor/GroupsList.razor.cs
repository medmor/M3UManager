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
        string sortOption = "count"; // default: count, name, original

        protected override void OnInitialized()
        {
            var model = m3UService.GetModel(M3UListModelId);
            filtredGroups = model.M3UGroups
                .Where(g => IsGroupMatchingContentFilter(g.Value))
                .ToDictionary(g => g.Key, g => g.Value);
            ApplySorting();
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
                    .ToDictionary(g => g.Key, g => g.Value);
            }
            else
            {
                filtredGroups = model.M3UGroups
                    .Where(g => IsGroupMatchingContentFilter(g.Value) && 
                               g.Value.Name.Contains(groupFilterString, System.StringComparison.CurrentCultureIgnoreCase))
                    .ToDictionary(g => g.Key, g => g.Value);
            }
            ApplySorting();
        }

        void ApplySorting()
        {
            var sorted = sortOption switch
            {
                "name" => filtredGroups.OrderBy(g => GetDisplayGroupName(g.Value.Name)),
                "original" => filtredGroups.OrderBy(g => g.Key),
                _ => filtredGroups.OrderByDescending(g => g.Value.Channels.Count) // "count" is default
            };
            filtredGroups = sorted.ToDictionary(g => g.Key, g => g.Value);
        }

        // Removed comparison feature - single playlist only
        bool IsInBoothLists(string gTrimmedName) => false;

        private string GetDisplayGroupName(string groupName)
        {
            // Remove content type prefixes from display
            var prefixes = new[]
            {
                "📺 Live TV - ",
                "🎬 Movies - ",
                "📺 TV Shows - ",
                "Live TV - ",
                "Movies - ",
                "TV Shows - "
            };

            foreach (var prefix in prefixes)
            {
                if (groupName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return groupName.Substring(prefix.Length);
                }
            }

            return groupName;
        }
    }
}
