using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ПрогЛабор.Models;
using ПрогЛабор.Services;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LaboratoryApp.ViewModels
{
    public partial class LabDecisionViewModel : ObservableObject
    {
        private readonly ILabRepository _labRepository;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;
        private readonly bool _isRawMaterial;
        private readonly int _batchId;

        [ObservableProperty]
        private string _decision = "approved";

        [ObservableProperty]
        private string? _reason;

        [ObservableProperty]
        private bool _canDecide = true;

        public ICommand SubmitDecisionCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler? CloseRequest;

        public LabDecisionViewModel(
            ILabRepository labRepository,
            IDialogService dialogService,
            IAuthService authService,
            bool isRawMaterial,
            int batchId)
        {
            _labRepository = labRepository;
            _dialogService = dialogService;
            _authService = authService;
            _isRawMaterial = isRawMaterial;
            _batchId = batchId;

            SubmitDecisionCommand = new AsyncRelayCommand(SubmitDecisionAsync, () => CanDecide);
            CancelCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));
        }

        private async Task SubmitDecisionAsync()
        {
            if (Decision == "blocked" && string.IsNullOrWhiteSpace(Reason))
            {
                _dialogService.ShowMessage("При блокировке партии необходимо указать причину");
                return;
            }

            CanDecide = false;
            try
            {
                bool success;
                if (_isRawMaterial)
                {
                    success = await _labRepository.SetRawMaterialBatchDecisionAsync(_batchId, Decision, Reason, _authService.CurrentUser!.Id);
                }
                else
                {
                    success = await _labRepository.SetProductBatchDecisionAsync(_batchId, Decision, Reason, _authService.CurrentUser!.Id);
                }
                if (success)
                {
                    CloseRequest?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _dialogService.ShowMessage("Не удалось принять решение");
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
            finally
            {
                CanDecide = true;
            }
        }
    }
}