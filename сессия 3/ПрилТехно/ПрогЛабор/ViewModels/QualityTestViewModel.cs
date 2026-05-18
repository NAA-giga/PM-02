using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ПрогЛабор.Models;
using ПрогЛабор.Repositories;
using ПрогЛабор.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace LaboratoryApp.ViewModels
{
    public partial class QualityTestViewModel : ObservableObject
    {
        private readonly ILabRepository _labRepository;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;
        private readonly IEventLogger _eventLogger;
        private readonly int _testId;

        [ObservableProperty]
        private QualityTestDto _test = null!;

        [ObservableProperty]
        private ObservableCollection<QualityTestResultDto> _parameters = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isCompleted;

        public ICommand SaveAllResultsCommand { get; }
        public ICommand CompleteTestCommand { get; }
        public ICommand ExportProtocolCommand { get; }

        public QualityTestViewModel(
            ILabRepository labRepository,
            IDialogService dialogService,
            IAuthService authService,
            IEventLogger eventLogger,
            int testId)
        {
            _labRepository = labRepository;
            _dialogService = dialogService;
            _authService = authService;
            _eventLogger = eventLogger;
            _testId = testId;

            SaveAllResultsCommand = new AsyncRelayCommand(SaveAllResultsAsync);
            CompleteTestCommand = new AsyncRelayCommand(CompleteTestAsync, () => !IsCompleted);
            ExportProtocolCommand = new AsyncRelayCommand(ExportProtocolAsync);

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                Test = await _labRepository.GetQualityTestByIdAsync(_testId);
                if (Test == null)
                {
                    _dialogService.ShowMessage("Испытание не найдено");
                    return;
                }
                IsCompleted = Test.Status == "completed";
                var results = await _labRepository.GetQualityTestResultsAsync(_testId);
                Parameters = new ObservableCollection<QualityTestResultDto>(results);
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

        private async Task SaveAllResultsAsync()
        {
            foreach (var param in Parameters)
            {
                if (param.MeasuredValue.HasValue)
                {
                    await _labRepository.SaveQualityTestResultAsync(param);
                }
            }
            _dialogService.ShowMessage("Результаты сохранены");
        }

        private async Task CompleteTestAsync()
        {
            if (IsCompleted)
            {
                _dialogService.ShowMessage("Испытание уже завершено");
                return;
            }
            try
            {
                var success = await _labRepository.CompleteQualityTestAsync(_testId, _authService.CurrentUser!.Id);
                if (success)
                {
                    IsCompleted = true;
                    _dialogService.ShowMessage("Испытание завершено");
                }
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

        private async Task ExportProtocolAsync()
        {
            // TODO: экспорт в Excel
            _dialogService.ShowMessage("Формирование протокола...");
            await Task.Delay(100);
        }
    }
}