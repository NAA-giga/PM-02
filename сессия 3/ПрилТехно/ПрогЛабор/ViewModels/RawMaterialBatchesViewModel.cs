using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaboratoryApp.Models;
using LaboratoryApp.Repositories;
using LaboratoryApp.Services;
using LaboratoryApp.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ПрогЛабор.Models;
using ПрогЛабор.Services;

namespace LaboratoryApp.ViewModels
{
    public partial class RawMaterialBatchesViewModel : ObservableObject
    {
        private readonly ILabRepository _labRepository;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;
        private readonly IEventLogger _eventLogger;

        [ObservableProperty]
        private ObservableCollection<RawMaterialBatchDto> _batches = new();

        [ObservableProperty]
        private RawMaterialBatchDto? _selectedBatch;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _searchText = string.Empty;

        public ICommand LoadBatchesCommand { get; }
        public ICommand StartTestCommand { get; }
        public ICommand ViewTestCommand { get; }
        public ICommand MakeDecisionCommand { get; }
        public ICommand RefreshCommand { get; }

        public RawMaterialBatchesViewModel(
            ILabRepository labRepository,
            IDialogService dialogService,
            IAuthService authService,
            IEventLogger eventLogger)
        {
            _labRepository = labRepository;
            _dialogService = dialogService;
            _authService = authService;
            _eventLogger = eventLogger;

            LoadBatchesCommand = new AsyncRelayCommand(LoadBatchesAsync);
            StartTestCommand = new AsyncRelayCommand(StartTestAsync, () => SelectedBatch != null);
            ViewTestCommand = new AsyncRelayCommand(ViewTestAsync, () => SelectedBatch != null && SelectedBatch.LastTestId.HasValue);
            MakeDecisionCommand = new AsyncRelayCommand(MakeDecisionAsync, () => SelectedBatch != null && SelectedBatch.LabStatus != "approved" && SelectedBatch.LabStatus != "blocked");
            RefreshCommand = new AsyncRelayCommand(LoadBatchesAsync);
        }

        private async Task LoadBatchesAsync()
        {
            IsLoading = true;
            try
            {
                var batches = await _labRepository.GetRawMaterialBatchesAsync();
                Batches = new ObservableCollection<RawMaterialBatchDto>(batches);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // уже загружены все
                return;
            }
            var filtered = Batches.Where(b => b.BatchNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                               b.RawMaterialName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            Batches = new ObservableCollection<RawMaterialBatchDto>(filtered);
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        private async Task StartTestAsync()
        {
            if (SelectedBatch == null) return;
            try
            {
                // Создаём новое испытание
                var testId = await _labRepository.CreateRawMaterialTestAsync(SelectedBatch.Id, "input_control", _authService.CurrentUser!.Id);
                // Открываем окно испытания
                var testViewModel = new RawMaterialTestViewModel(_labRepository, _dialogService, _authService, _eventLogger, testId);
                var testWindow = new RawMaterialTestView(testViewModel);
                testWindow.ShowDialog();
                await LoadBatchesAsync(); // обновляем список после закрытия
            }
            catch (InvalidOperationException ex)
            {
                _dialogService.ShowMessage(ex.Message);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}");
            }
        }

        private async Task ViewTestAsync()
        {
            if (SelectedBatch?.LastTestId == null) return;
            var testViewModel = new RawMaterialTestViewModel(_labRepository, _dialogService, _authService, _eventLogger, SelectedBatch.LastTestId.Value);
            var testWindow = new RawMaterialTestView(testViewModel);
            testWindow.ShowDialog();
            await LoadBatchesAsync();
        }

        private async Task MakeDecisionAsync()
        {
            if (SelectedBatch == null) return;
            var decisionViewModel = new LabDecisionViewModel(_labRepository, _dialogService, _authService, true, SelectedBatch.Id);
            var decisionWindow = new LabDecisionView(decisionViewModel);
            if (decisionWindow.ShowDialog() == true)
                await LoadBatchesAsync();
        }
    }
}