using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ПрилТехно.Services;

namespace ПрилТехно.ViewModels
{
    public partial class RecipesViewModel : ObservableObject
    {
        private readonly ApiService _api;

        [ObservableProperty]
        private ObservableCollection<RecipeDto> _recipes = [];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string? _statusFilter;

        [ObservableProperty]
        private int? _productFilter;

        [ObservableProperty]
        private RecipeDto? _selectedRecipe;

        public ObservableCollection<ProductDto> Products { get; } = [];

        public RecipesViewModel(ApiService api)
        {
            _api = api;
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            NewRecipeCommand = new AsyncRelayCommand(NewRecipeAsync);
            ApproveRecipeCommand = new AsyncRelayCommand<RecipeDto>(ApproveRecipeAsync, CanModifyRecipe);
            ArchiveRecipeCommand = new AsyncRelayCommand<RecipeDto>(ArchiveRecipeAsync, CanModifyRecipe);
            EditRecipeCommand = new RelayCommand<RecipeDto>(EditRecipe);
            ApplyFilterCommand = new AsyncRelayCommand(ApplyFilterAsync);
        }

        // Команды
        public IAsyncRelayCommand LoadDataCommand { get; }
        public IAsyncRelayCommand NewRecipeCommand { get; }
        public IAsyncRelayCommand<RecipeDto> ApproveRecipeCommand { get; }
        public IAsyncRelayCommand<RecipeDto> ArchiveRecipeCommand { get; }
        public IRelayCommand<RecipeDto> EditRecipeCommand { get; }
        public IAsyncRelayCommand ApplyFilterCommand { get; }

        private async Task LoadDataAsync()
        {
            if (IsLoading) return;
            try
            {
                IsLoading = true;
                var products = await _api.GetProductsAsync();
                Products.Clear();
                foreach (var p in products)
                    Products.Add(p);

                await ApplyFilterAsync();
            }
            catch (Exception ex)
            {
                // Показать сообщение об ошибке (через событие или сервис диалогов)
                System.Windows.MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ApplyFilterAsync()
        {
            try
            {
                IsLoading = true;
                var items = await _api.GetRecipesAsync(ProductFilter, StatusFilter);
                Recipes.Clear();
                foreach (var r in items)
                    Recipes.Add(r);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task NewRecipeAsync()
        {
            // Открыть окно создания новой рецептуры (через сервис или прямое создание окна)
            var win = new Views.RecipeEditWindow(_api, null);
            if (win.ShowDialog() == true)
                await LoadDataAsync();
        }

        private void EditRecipe(RecipeDto? recipe)
        {
            if (recipe is null) return;
            var win = new Views.RecipeEditWindow(_api, recipe.Id);
            win.ShowDialog();
            // Обновить список, если были изменения (можно перезагрузить текущую страницу)
            _ = LoadDataAsync();
        }

        private async Task ApproveRecipeAsync(RecipeDto? recipe)
        {
            if (recipe is null) return;
            if (System.Windows.MessageBox.Show($"Утвердить рецептуру '{recipe.Name}'?", "Подтверждение",
                System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes)
                return;

            try
            {
                await _api.ApproveRecipeAsync(recipe.Id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка утверждения: {ex.Message}", "Ошибка");
            }
        }

        private async Task ArchiveRecipeAsync(RecipeDto? recipe)
        {
            if (recipe is null) return;
            if (System.Windows.MessageBox.Show($"Архивировать рецептуру '{recipe.Name}'?", "Подтверждение",
                System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes)
                return;

            try
            {
                await _api.ArchiveRecipeAsync(recipe.Id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка архивирования: {ex.Message}", "Ошибка");
            }
        }

        private bool CanModifyRecipe(RecipeDto? recipe) =>
            recipe is not null && recipe.Status != "approved" && recipe.Status != "archived";
    }
}
