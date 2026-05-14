using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ПрилТехно.Models;
using ПрилТехно.Services;
using ПрилТехно.Views;

namespace ПрилТехно.ViewModels
{
    public partial class TechCardEditViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly IDialogService _dialogService;
        private readonly int? _cardId;

        [ObservableProperty]
        private ObservableCollection<ProductDto> _products = new();

        [ObservableProperty]
        private ProductDto? _selectedProduct;

        [ObservableProperty]
        private string _techCardName = string.Empty;

        [ObservableProperty]
        private string? _description;

        [ObservableProperty]
        private ObservableCollection<TechStepDto> _steps = new();

        [ObservableProperty]
        private TechStepDto? _selectedStep;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isProductSelectable = true; // для новой карты можно выбрать, для редактирования - нет

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddStepCommand { get; }
        public ICommand EditStepCommand { get; }
        public ICommand RemoveStepCommand { get; }

        public event EventHandler? CloseRequest;

        public TechCardEditViewModel(ApiClient apiClient, IDialogService dialogService, int? cardId = null)
        {
            _apiClient = apiClient;
            _dialogService = dialogService;
            _cardId = cardId;

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));
            AddStepCommand = new AsyncRelayCommand(AddStepAsync);
            EditStepCommand = new AsyncRelayCommand<TechStepDto?>(EditStepAsync);
            RemoveStepCommand = new AsyncRelayCommand<TechStepDto?>(RemoveStepAsync);

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                // Загрузка списка продуктов для выбора
                var productsResponse = await _apiClient.GetAsync<List<ProductDto>>("/api/reference/products");
                if (productsResponse?.IsSuccess == true && productsResponse.Data != null)
                {
                    Products = new ObservableCollection<ProductDto>(productsResponse.Data);
                }

                if (_cardId.HasValue)
                {
                    // Режим редактирования – загружаем карту
                    var cardResponse = await _apiClient.GetAsync<TechCardDto>($"/api/techcards/{_cardId}");
                    if (cardResponse?.IsSuccess == true && cardResponse.Data != null)
                    {
                        var card = cardResponse.Data;
                        SelectedProduct = Products.FirstOrDefault(p => p.Id == card.ProductId);
                        TechCardName = card.Name;
                        Description = card.Description;
                        Steps = new ObservableCollection<TechStepDto>(card.Steps.OrderBy(s => s.StepOrder));
                        IsProductSelectable = false; // продукт нельзя менять у существующей карты
                    }
                    else
                    {
                        _dialogService.ShowMessage("Не удалось загрузить карту", "Ошибка");
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

        private async Task AddStepAsync()
        {
            // Открываем окно редактирования шага без переданного шага (создание нового)
            var stepVm = new StepEditViewModel(_apiClient, _dialogService);
            var stepWindow = new StepEditView(stepVm);
            if (stepWindow.ShowDialog() == true && stepVm.Step != null)
            {
                // Для нового шага временный Id = 0, порядок = текущее количество + 1
                stepVm.Step.Id = 0;
                stepVm.Step.TechCardId = _cardId ?? 0;
                stepVm.Step.StepOrder = Steps.Count + 1;
                Steps.Add(stepVm.Step);
            }
        }

        private async Task EditStepAsync(TechStepDto? step)
        {
            if (step == null) return;
            // Открываем окно редактирования существующего шага
            var stepVm = new StepEditViewModel(_apiClient, _dialogService, step);
            var stepWindow = new StepEditView(stepVm);
            if (stepWindow.ShowDialog() == true && stepVm.Step != null)
            {
                var index = Steps.IndexOf(step);
                if (index >= 0)
                    Steps[index] = stepVm.Step;
            }
        }

        private async Task RemoveStepAsync(TechStepDto? step)
        {
            if (step == null) return;
            if (_dialogService.ShowConfirmation($"Удалить шаг '{step.StepName}'?", "Подтверждение"))
            {
                Steps.Remove(step);
                // Перенумеровать порядок шагов
                for (int i = 0; i < Steps.Count; i++)
                    Steps[i].StepOrder = i + 1;
            }
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(TechCardName))
            {
                _dialogService.ShowMessage("Укажите название технологической карты", "Ошибка");
                return;
            }
            if (!_cardId.HasValue && SelectedProduct == null)
            {
                _dialogService.ShowMessage("Выберите продукт", "Ошибка");
                return;
            }

            IsLoading = true;
            try
            {
                int cardId;
                if (_cardId.HasValue)
                {
                    // Обновление основной информации карты
                    var updateDto = new { name = TechCardName, description = Description };
                    var updateResponse = await _apiClient.PutAsync<object>($"/api/techcards/{_cardId}", updateDto);
                    if (updateResponse?.IsSuccess != true)
                    {
                        _dialogService.ShowMessage(updateResponse?.ErrorMessage ?? "Ошибка обновления карты", "Ошибка");
                        return;
                    }
                    cardId = _cardId.Value;

                    // Обновление шагов: 
                    // 1) Получаем текущие шаги из API
                    var existingCard = await _apiClient.GetAsync<TechCardDto>($"/api/techcards/{cardId}");
                    var existingSteps = existingCard?.Data?.Steps ?? new List<TechStepDto>();
                    // 2) Удаляем шаги, которых нет в текущем списке
                    foreach (var oldStep in existingSteps)
                    {
                        if (!Steps.Any(s => s.Id == oldStep.Id))
                        {
                            await _apiClient.DeleteAsync<object>($"/api/techcards/steps/{oldStep.Id}");
                        }
                    }
                    // 3) Добавляем новые шаги и обновляем существующие
                    foreach (var step in Steps)
                    {
                        if (step.Id == 0)
                        {
                            // Новый шаг
                            await _apiClient.PostAsync<object>($"/api/techcards/{cardId}/steps", step);
                        }
                        else
                        {
                            // Обновление существующего
                            await _apiClient.PutAsync<object>($"/api/techcards/steps/{step.Id}", step);
                        }
                    }
                }
                else
                {
                    // Создание новой карты
                    var createDto = new
                    {
                        productId = SelectedProduct!.Id,
                        name = TechCardName,
                        description = Description,
                        steps = Steps.Select(s => new
                        {
                            s.StepOrder,
                            s.StepName,
                            s.StepType,
                            s.EquipmentId,
                            s.PlannedTempC,
                            s.PlannedPressureBar,
                            s.PlannedDurationMin,
                            s.PlannedSpeedRpm,
                            s.TempToleranceMin,
                            s.TempToleranceMax,
                            s.PressureToleranceMin,
                            s.PressureToleranceMax,
                            s.IsMandatory,
                            s.Instruction
                        }).ToList()
                    };
                    var createResponse = await _apiClient.PostAsync<dynamic>("/api/techcards", createDto);
                    if (createResponse?.IsSuccess != true)
                    {
                        _dialogService.ShowMessage(createResponse?.ErrorMessage ?? "Ошибка создания карты", "Ошибка");
                        return;
                    }
                    cardId = createResponse.Data?.Id;
                }

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
    }
}