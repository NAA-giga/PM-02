using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ПрилТехно.Models;
using ПрилТехно.Services;
using ПрилТехно.Views;
namespace ПрилТехно.ViewModels
{
    public partial class RecipesViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<RecipeDto> _recipes = new();

        [ObservableProperty]
        private bool _isLoading;

        public ICommand LoadRecipesCommand { get; }
        public ICommand CreateRecipeCommand { get; }
        public ICommand EditRecipeCommand { get; }
        public ICommand ApproveRecipeCommand { get; }
        public ICommand ArchiveRecipeCommand { get; }
        public ICommand RefreshCommand { get; }

        public RecipesViewModel(ApiClient apiClient, IDialogService dialogService)
        {
            _apiClient = apiClient;
            _dialogService = dialogService;

            LoadRecipesCommand = new AsyncRelayCommand(LoadRecipesAsync);
            CreateRecipeCommand = new AsyncRelayCommand(CreateRecipeAsync);
            EditRecipeCommand = new AsyncRelayCommand<RecipeDto?>(EditRecipeAsync);
            ApproveRecipeCommand = new AsyncRelayCommand<RecipeDto?>(ApproveRecipeAsync);
            ArchiveRecipeCommand = new AsyncRelayCommand<RecipeDto?>(ArchiveRecipeAsync);
            RefreshCommand = new AsyncRelayCommand(LoadRecipesAsync);
        }

        private async Task LoadRecipesAsync()
        {
            IsLoading = true;
            try
            {
                var response = await _apiClient.GetAsync<List<RecipeDto>>("/api/recipes");
                if (response?.IsSuccess == true && response.Data != null)
                    Recipes = new ObservableCollection<RecipeDto>(response.Data);
                else
                    _dialogService.ShowMessage(response?.ErrorMessage ?? "Ошибка загрузки", "Ошибка");
            }
            catch (Exception ex) { _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка"); }
            finally { IsLoading = false; }
        }

        private async Task CreateRecipeAsync()
        {
            var editVm = new RecipeEditViewModel(_apiClient, _dialogService);
            var editWindow = new RecipeEditView(editVm);
            if (editWindow.ShowDialog() == true)
                await LoadRecipesAsync();
        }

        private async Task EditRecipeAsync(RecipeDto? recipe)
        {
            if (recipe == null) return;
            var editVm = new RecipeEditViewModel(_apiClient, _dialogService, recipe.Id);
            var editWindow = new RecipeEditView(editVm);
            if (editWindow.ShowDialog() == true)
                await LoadRecipesAsync();
        }

        private async Task ApproveRecipeAsync(RecipeDto? recipe)
        {
            if (recipe == null) return;
            if (!_dialogService.ShowConfirmation($"Утвердить рецептуру '{recipe.Name}'?", "Подтверждение"))
                return;
            try
            {
                var response = await _apiClient.PostAsync<object>($"/api/recipes/{recipe.Id}/approve", null);
                if (response?.IsSuccess == true)
                    await LoadRecipesAsync();
                else
                    _dialogService.ShowMessage(response?.ErrorMessage ?? "Ошибка утверждения", "Ошибка");
            }
            catch (Exception ex) { _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка"); }
        }

        private async Task ArchiveRecipeAsync(RecipeDto? recipe)
        {
            if (recipe == null) return;
            if (!_dialogService.ShowConfirmation($"Архивировать рецептуру '{recipe.Name}'?", "Подтверждение"))
                return;
            try
            {
                var response = await _apiClient.PostAsync<object>($"/api/recipes/{recipe.Id}/archive", null);
                if (response?.IsSuccess == true)
                    await LoadRecipesAsync();
                else
                    _dialogService.ShowMessage(response?.ErrorMessage ?? "Ошибка архивации", "Ошибка");
            }
            catch (Exception ex) { _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка"); }
        }
    }
}