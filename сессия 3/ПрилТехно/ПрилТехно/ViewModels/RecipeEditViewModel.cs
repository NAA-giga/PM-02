using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ПрилТехно.Models;
using ПрилТехно.Services;
using ПрилТехно.Views;

namespace ПрилТехно.ViewModels
{
    public partial class RecipeEditViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly IDialogService _dialogService;
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
        private bool _isProductSelectable = true; // для новой рецептуры можно выбрать продукт, при редактировании - нельзя

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddComponentCommand { get; }
        public ICommand EditComponentCommand { get; }
        public ICommand RemoveComponentCommand { get; }

        public event EventHandler? CloseRequest;

        public RecipeEditViewModel(ApiClient apiClient, IDialogService dialogService, int? recipeId = null)
        {
            _apiClient = apiClient;
            _dialogService = dialogService;
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
                // Загружаем список продуктов для выбора
                var productsResponse = await _apiClient.GetAsync<List<ProductDto>>("/api/reference/products");
                if (productsResponse?.IsSuccess == true && productsResponse.Data != null)
                {
                    Products = new ObservableCollection<ProductDto>(productsResponse.Data);
                }

                // Загружаем список сырья для выбора в компонентах (хранится в сервисе или отдельно)
                // Для компонентов нам нужны все активные материалы – загружаем их в статическое поле или прямо здесь.
                // Лучше загружать отдельно при открытии окна компонента.

                if (_recipeId.HasValue)
                {
                    // Режим редактирования
                    var recipeResponse = await _apiClient.GetAsync<RecipeDto>($"/api/recipes/{_recipeId}");
                    if (recipeResponse?.IsSuccess == true && recipeResponse.Data != null)
                    {
                        var recipe = recipeResponse.Data;
                        SelectedProduct = Products.FirstOrDefault(p => p.Id == recipe.ProductId);
                        RecipeName = recipe.Name;
                        RecipeVersion = recipe.Version;
                        Components = new ObservableCollection<RecipeComponentDto>(recipe.Components);
                        IsProductSelectable = false; // продукт нельзя менять у существующей рецептуры
                    }
                    else
                    {
                        _dialogService.ShowMessage("Не удалось загрузить рецептуру", "Ошибка");
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

        private async Task AddComponentAsync()
        {
            var materials = await LoadRawMaterialsAsync();
            if (!materials.Any())
            {
                _dialogService.ShowMessage("Нет доступного сырья для выбора", "Ошибка");
                return;
            }
            var vm = new ComponentEditViewModel(materials);
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
            var materials = await LoadRawMaterialsAsync();
            var vm = new ComponentEditViewModel(materials, component);
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
                // Перенумеровать LoadOrder
                for (int i = 0; i < Components.Count; i++)
                    Components[i].LoadOrder = i + 1;
            }
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(RecipeName))
            {
                _dialogService.ShowMessage("Укажите название рецептуры", "Ошибка");
                return;
            }
            if (SelectedProduct == null && !_recipeId.HasValue)
            {
                _dialogService.ShowMessage("Выберите продукт", "Ошибка");
                return;
            }
            if (Components.Count == 0)
            {
                _dialogService.ShowMessage("Добавьте хотя бы один компонент", "Ошибка");
                return;
            }

            // Проверка суммы процентов
            var total = Components.Sum(c => c.Percentage);
            if (Math.Abs(total - 100) > 0.01m)
            {
                _dialogService.ShowMessage($"Сумма компонентов должна быть 100%, сейчас {total}%", "Ошибка");
                return;
            }

            // Формируем объект для отправки
            object payload;
            string url;
            if (_recipeId.HasValue)
            {
                url = $"/api/recipes/{_recipeId}";
                payload = new { name = RecipeName }; // обновляем только название
                // компоненты обновляются отдельными запросами? Упростим: сначала удалим старые, потом добавим новые.
                // Однако API поддерживает отдельные методы для компонентов, поэтому лучше отправлять компоненты по одному.
                // Для упрощения в рамках спринта можно создать метод PUT /api/recipes/{id}/components (перезапись всех компонентов).
                // Но мы воспользуемся существующими методами: удалим все старые компоненты и добавим новые.
                // Для этого нужно получить список id старых компонентов и удалить их.
            }
            else
            {
                url = "/api/recipes";
                payload = new
                {
                    productId = SelectedProduct!.Id,
                    name = RecipeName
                    // version не передаём – сервер сам вычислит
                };
            }

            try
            {
                IsLoading = true;
                ApiResponse<object>? response;
                if (_recipeId.HasValue)
                    response = await _apiClient.PutAsync<object>(url, payload);
                else
                    response = await _apiClient.PostAsync<object>(url, payload);

                if (response?.IsSuccess != true)
                {
                    _dialogService.ShowMessage(response?.ErrorMessage ?? "Ошибка сохранения рецептуры", "Ошибка");
                    return;
                }

                int recipeId = _recipeId ?? (int)response.Data?.GetType().GetProperty("Id")?.GetValue(response.Data, null);

                // Сохраняем компоненты (сначала удаляем старые, если редактирование)
                if (_recipeId.HasValue)
                {
                    // Получаем текущие компоненты из API и удаляем их
                    var existing = await _apiClient.GetAsync<RecipeDto>($"/api/recipes/{recipeId}");
                    if (existing?.IsSuccess == true && existing.Data != null)
                    {
                        foreach (var comp in existing.Data.Components)
                        {
                            await _apiClient.DeleteAsync<object>($"/api/recipes/components/{comp.Id}");
                        }
                    }
                }
                // Добавляем новые компоненты
                foreach (var comp in Components)
                {
                    var compPayload = new
                    {
                        rawMaterialId = comp.RawMaterialId,
                        percentage = comp.Percentage,
                        loadOrder = comp.LoadOrder,
                        toleranceMin = comp.ToleranceMin,
                        toleranceMax = comp.ToleranceMax
                    };
                    await _apiClient.PostAsync<object>($"/api/recipes/{recipeId}/components", compPayload);
                }

                CloseRequest?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<List<RawMaterialDto>> LoadRawMaterialsAsync()
        {
            var response = await _apiClient.GetAsync<List<RawMaterialDto>>("/api/reference/materials");
            return response?.IsSuccess == true && response.Data != null ? response.Data : new List<RawMaterialDto>();
        }
    }
}