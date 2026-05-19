using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using прогОпер.Models;
using прогОпер.Repositories;
using прогОпер.Services;
using прогОпер.Views;

namespace прогОпер.ViewModels
{
    public partial class BatchExecutionViewModel : ObservableObject
    {
        private readonly IOperatorRepository _repository;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private BatchDetailsDto? _batch;

        [ObservableProperty]
        private ObservableCollection<StepExecutionDto> _steps = new();

        [ObservableProperty]
        private StepExecutionDto? _selectedStep;

        [ObservableProperty]
        private bool _isLoading;

        public ICommand GoBackCommand { get; }

        public BatchExecutionViewModel(
            IOperatorRepository repository,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _repository = repository;
            _dialogService = dialogService;
            _navigationService = navigationService;
            GoBackCommand = new RelayCommand(GoBack);
        }

        public async Task InitializeAsync(int batchId)
        {
            await LoadBatchInternalAsync(batchId);
        }

        private async Task LoadBatchInternalAsync(int batchId)
        {
            IsLoading = true;
            try
            {
                var batchData = await _repository.GetBatchDetailsAsync(batchId);
                if (batchData == null)
                {
                    _dialogService.ShowMessage("Партия не найдена");
                    GoBack();
                    return;
                }
                Batch = batchData;
                Steps = new ObservableCollection<StepExecutionDto>(batchData.Steps);
                SelectedStep = Steps.FirstOrDefault(s => s.Status != "completed");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки: {ex.Message}");
                GoBack();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void GoBack()
        {
            _navigationService.NavigateTo<ActiveBatchesView, ActiveBatchesViewModel>();
        }
    }
}