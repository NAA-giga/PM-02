using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dapper;
using ПрилТехно.Models;
using ПрилТехно.Repositories;
using ПрилТехно.Services;
using ПрилТехно.Views;

namespace ПрилТехно.ViewModels
{
    public partial class RecipeEditViewModel : ObservableObject
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;
        private readonly int? _recipeId;

        [ObservableProperty]
        private ObservableCollection<ProductDto> _products = new();

        [ObservableProperty]
        private ProductDto? _selectedProduct;

        [ObservableProperty]
        private string _recipeName = string.Empty;

        [ObservableProperty]
        private int _recipeVersion;

        [ObservableProperty]
        private ObservableCollection<RecipeComponentDto> _components = new();

        [ObservableProperty]
        private RecipeComponentDto? _selectedComponent;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isProductSelectable = true;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddComponentCommand { get; }
        public ICommand EditComponentCommand { get; }
        public ICommand RemoveComponentCommand { get; }

        public event EventHandler? CloseRequest;

        public RecipeEditViewModel(
            IRecipeRepository recipeRepository,
            IDbConnectionFactory connectionFactory,
            IDialogService dialogService,
            IAuthService authService,
            int? recipeId = null)
        {
            _recipeRepository = recipeRepository;
            _connectionFactory = connectionFactory;
            _dialogService = dialogService;
            _authService = authService;
            _recipeId = recipeId;

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));
            AddComponentCommand = new AsyncRelayCommand(AddComponentAsync);
            EditComponentCommand = new AsyncRelayCommand<RecipeComponentDto?>(EditComponentAsync);
            RemoveComponentCommand = new AsyncRelayCommand<RecipeComponentDto?>(RemoveComponentAsync);

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                // Загружаем список активных продуктов
                using var conn = _connectionFactory.CreateConnection();
                var products = await conn.QueryAsync<ProductDto>(
                    "SELECT id, code, name, product_type, form_type, status FROM products WHERE status = 'active'");
                Products = new ObservableCollection<ProductDto>(products);

                if (_recipeId.HasValue)
                {
                    var recipe = await _recipeRepository.GetRecipeByIdAsync(_recipeId.Value);
                    if (recipe != null)
                    {
                        SelectedProduct = Products.FirstOrDefault(p => p.Id == recipe.ProductId);
                        RecipeName = recipe.Name;
                        RecipeVersion = recipe.Version;
                        Components = new ObservableCollection<RecipeComponentDto>(recipe.Components);
                        IsProductSelectable = false;
                    }
                    else
                    {
                        _dialogService.ShowMessage("Рецептура не найдена", "Ошибка");
                        CloseRequest?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки: {ex.Message}", "Ошибка");
                CloseRequest?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(RecipeName))
            {
                _dialogService.ShowMessage("Укажите название рецептуры", "Ошибка");
                return;
            }
            if (!_recipeId.HasValue && SelectedProduct == null)
            {
                _dialogService.ShowMessage("Выберите продукт", "Ошибка");
                return;
            }
            if (Components.Count == 0)
            {
                _dialogService.ShowMessage("Добавьте хотя бы один компонент", "Ошибка");
                return;
            }

            var total = Components.Sum(c => c.Percentage);
            if (Math.Abs(total - 100) > 0.01m)
            {
                _dialogService.ShowMessage($"Сумма процентов должна быть 100%, сейчас {total}%", "Ошибка");
                return;
            }

            var recipeDto = new RecipeDto
            {
                Id = _recipeId ?? 0,
                ProductId = SelectedProduct!.Id,
                ProductName = SelectedProduct.Name,
                Version = _recipeId.HasValue ? RecipeVersion : 0, // для новой рецептуры версия будет вычислена в репозитории
                Name = RecipeName,
                Status = "draft",
                CreatedBy = _authService.CurrentUser!.Id,
                Components = Components.ToList()
            };

            IsLoading = true;
            try
            {
                if (_recipeId.HasValue)
                    await _recipeRepository.UpdateRecipeAsync(recipeDto);
                else
                    await _recipeRepository.CreateRecipeAsync(recipeDto);
                CloseRequest?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка сохранения: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddComponentAsync()
        {
            // Загружаем сырьё для выбора в компоненте
            using var conn = _connectionFactory.CreateConnection();
            var materials = await conn.QueryAsync<RawMaterialDto>(
                "SELECT id, code, name, category, unit_of_measure FROM raw_materials WHERE is_active = 1 ORDER BY name");
            var vm = new ComponentEditViewModel(materials.ToList());
            var win = new ComponentEditView(vm);
            if (win.ShowDialog() == true && vm.Component != null)
            {
                vm.Component.LoadOrder = Components.Count + 1;
                Components.Add(vm.Component);
            }
        }

        private async Task EditComponentAsync(RecipeComponentDto? component)
        {
            if (component == null) return;
            using var conn = _connectionFactory.CreateConnection();
            var materials = await conn.QueryAsync<RawMaterialDto>(
                "SELECT id, code, name, category, unit_of_measure FROM raw_materials WHERE is_active = 1 ORDER BY name");
            var vm = new ComponentEditViewModel(materials.ToList(), component);
            var win = new ComponentEditView(vm);
            if (win.ShowDialog() == true && vm.Component != null)
            {
                var index = Components.IndexOf(component);
                if (index >= 0)
                    Components[index] = vm.Component;
            }
        }

        private async Task RemoveComponentAsync(RecipeComponentDto? component)
        {
            if (component == null) return;
            if (_dialogService.ShowConfirmation($"Удалить компонент '{component.RawMaterialName}'?", "Подтверждение"))
            {
                Components.Remove(component);
                // Перенумеровать порядок загрузки
                for (int i = 0; i < Components.Count; i++)
                    Components[i].LoadOrder = i + 1;
            }
        }
    }
}