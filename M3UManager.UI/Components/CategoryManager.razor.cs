using M3UManager.Models;
using M3UManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace M3UManager.UI.Components
{
    public partial class CategoryManager
    {
        [Inject] private IFavoritesService favoritesService { get; set; } = null!;

        [Parameter] public bool IsVisible { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback OnCategoriesChanged { get; set; }

        private List<FavoriteCategory> categories = new();
        private string newCategoryName = string.Empty;
        private string newCategoryIcon = "bi-star";
        private string? editingCategoryId = null;
        private string editingName = string.Empty;

        protected override void OnParametersSet()
        {
            if (IsVisible)
            {
                LoadCategories();
            }
        }

        private void LoadCategories()
        {
            categories = favoritesService.GetCategories();
            StateHasChanged();
        }

        private void AddCategory()
        {
            if (!string.IsNullOrWhiteSpace(newCategoryName))
            {
                favoritesService.AddCategory(newCategoryName.Trim(), newCategoryIcon);
                newCategoryName = string.Empty;
                newCategoryIcon = "bi-star";
                LoadCategories();
                _ = OnCategoriesChanged.InvokeAsync();
            }
        }

        private void StartEdit(string categoryId, string currentName)
        {
            editingCategoryId = categoryId;
            editingName = currentName;
        }

        private void SaveEdit(string categoryId)
        {
            if (!string.IsNullOrWhiteSpace(editingName))
            {
                favoritesService.RenameCategory(categoryId, editingName.Trim());
                editingCategoryId = null;
                editingName = string.Empty;
                LoadCategories();
                _ = OnCategoriesChanged.InvokeAsync();
            }
        }

        private void CancelEdit()
        {
            editingCategoryId = null;
            editingName = string.Empty;
        }

        private void DeleteCategory(string categoryId)
        {
            favoritesService.RemoveCategory(categoryId);
            LoadCategories();
            _ = OnCategoriesChanged.InvokeAsync();
        }

        private void HandleKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                AddCategory();
            }
        }

        private void HandleEditKeyPress(KeyboardEventArgs e, string categoryId)
        {
            if (e.Key == "Enter")
            {
                SaveEdit(categoryId);
            }
            else if (e.Key == "Escape")
            {
                CancelEdit();
            }
        }

        private async Task Close()
        {
            await OnClose.InvokeAsync();
        }
    }
}
