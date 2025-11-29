using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;

namespace M3UManager.UI.Components
{
    public partial class CategorySelector
    {
        [Inject] private IFavoritesService favoritesService { get; set; } = default!;

        [Parameter] public bool IsVisible { get; set; }
        [Parameter] public M3UChannel? Channel { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback<List<string>> OnCategoriesSelected { get; set; }

        private List<FavoriteCategory> categories = new();
        private HashSet<string> selectedCategoryIds = new();

        protected override void OnParametersSet()
        {
            if (IsVisible)
            {
                LoadCategoriesAndExisting();
            }
        }

        private void LoadCategoriesAndExisting()
        {
            categories = favoritesService.GetCategories();
            selectedCategoryIds.Clear();

            // Pre-select categories that already contain this channel
            if (Channel != null)
            {
                foreach (var category in categories)
                {
                    if (favoritesService.IsChannelInCategory(category.Id, Channel))
                    {
                        selectedCategoryIds.Add(category.Id);
                    }
                }
            }
        }

        private void ToggleCategorySelection(string categoryId, ChangeEventArgs e)
        {
            var isChecked = (bool)(e.Value ?? false);
            
            if (isChecked)
            {
                selectedCategoryIds.Add(categoryId);
            }
            else
            {
                selectedCategoryIds.Remove(categoryId);
            }
        }

        private void ToggleSelectAll(ChangeEventArgs e)
        {
            var isChecked = (bool)(e.Value ?? false);
            
            if (isChecked)
            {
                selectedCategoryIds = categories.Select(c => c.Id).ToHashSet();
            }
            else
            {
                selectedCategoryIds.Clear();
            }
        }

        private async Task SaveSelection()
        {
            if (selectedCategoryIds.Count > 0)
            {
                await OnCategoriesSelected.InvokeAsync(selectedCategoryIds.ToList());
            }
            await Close();
        }

        private async Task Cancel()
        {
            await Close();
        }

        private async Task Close()
        {
            selectedCategoryIds.Clear();
            await OnClose.InvokeAsync();
        }
    }
}
