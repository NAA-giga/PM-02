using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ПрилТехно.Models;
using ПрилТехно.Repositories;
using ПрилТехно.Services;
using ПрилТехно.Views;

namespace ПрилТехно.ViewModels
{
    public partial class RecipesViewModel : ObservableObject
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;

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

        public RecipesViewModel(
            IRecipeRepository recipeRepository,
            IDbConnectionFactory connectionFactory,
            IDialogService dialogService,
            IAuthService authService)
        {
            _recipeRepository = recipeRepository;
            _connectionFactory = connectionFactory;
            _dialogService = dialogService;
            _authService = authService;

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
                var list = await _recipeRepository.GetAllRecipesAsync();
                Recipes = new ObservableCollection<RecipeDto>(list);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки рецептур: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CreateRecipeAsync()
        {
            var editVm = new RecipeEditViewModel(_recipeRepository, _connectionFactory, _dialogService, _authService);
            var editWindow = new RecipeEditView(editVm);
            if (editWindow.ShowDialog() == true)
                await LoadRecipesAsync();
        }

        private async Task EditRecipeAsync(RecipeDto? recipe)
        {
            if (recipe == null) return;
            var editVm = new RecipeEditViewModel(_recipeRepository, _connectionFactory, _dialogService, _authService, recipe.Id);
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
                var success = await _recipeRepository.ApproveRecipeAsync(recipe.Id, _authService.CurrentUser!.Id);
                if (success)
                    await LoadRecipesAsync();
                else
                    _dialogService.ShowMessage("Не удалось утвердить рецептуру", "Ошибка");
            }
            catch (InvalidOperationException ex)
            {
                _dialogService.ShowMessage(ex.Message, "Ошибка");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private async Task ArchiveRecipeAsync(RecipeDto? recipe)
        {
            if (recipe == null) return;
            if (!_dialogService.ShowConfirmation($"Архивировать рецептуру '{recipe.Name}'?", "Подтверждение"))
                return;
            try
            {
                var success = await _recipeRepository.ArchiveRecipeAsync(recipe.Id);
                if (success)
                    await LoadRecipesAsync();
                else
                    _dialogService.ShowMessage("Не удалось архивировать рецептуру", "Ошибка");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
    }
}