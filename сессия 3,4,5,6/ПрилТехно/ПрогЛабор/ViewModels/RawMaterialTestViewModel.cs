using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ПрогЛабор.Models;
using ПрогЛабор.Services;

namespace ПрогЛабор.ViewModels
{
    public partial class RawMaterialTestViewModel : ObservableObject
    {
        private readonly ILabRepository _labRepository;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;
        private readonly IEventLogger _eventLogger;
        private readonly int _testId;
        private readonly IProtocolGenerator _protocolGenerator;

        [ObservableProperty]
        private RawMaterialTestDto _test = null!;

        [ObservableProperty]
        private ObservableCollection<RawMaterialTestResultDto> _parameters = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isCompleted;

        public ICommand SaveAllResultsCommand { get; }
        public ICommand CompleteTestCommand { get; }
        public ICommand ExportProtocolCommand { get; }

        // Событие для закрытия окна
        public event EventHandler? CloseRequest;

        public RawMaterialTestViewModel(
            ILabRepository labRepository,
            IDialogService dialogService,
            IAuthService authService,
            IEventLogger eventLogger,
            IProtocolGenerator protocolGenerator,
            int testId)
        {
            _labRepository = labRepository;
            _dialogService = dialogService;
            _authService = authService;
            _eventLogger = eventLogger;
            _protocolGenerator = protocolGenerator;
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
                Test = await _labRepository.GetRawMaterialTestByIdAsync(_testId);
                if (Test == null)
                {
                    _dialogService.ShowMessage("Испытание не найдено");
                    return;
                }
                IsCompleted = Test.Status == "completed";
                var results = await _labRepository.GetRawMaterialTestResultsAsync(_testId);
                Parameters = new ObservableCollection<RawMaterialTestResultDto>(results);
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
                    await _labRepository.SaveRawMaterialTestResultAsync(param);
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
                var success = await _labRepository.CompleteRawMaterialTestAsync(_testId, _authService.CurrentUser!.Id);
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
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"Протокол_{Test.TestNumber}_{DateTime.Now:yyyyMMdd_HHmmss}"
            };
            if (saveDialog.ShowDialog() != true) return;

            try
            {
                await _protocolGenerator.GenerateRawMaterialTestProtocolAsync(_testId, saveDialog.FileName);
                _dialogService.ShowMessage($"Протокол сохранён: {saveDialog.FileName}");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка при формировании протокола: {ex.Message}");
            }
        }
    }
}