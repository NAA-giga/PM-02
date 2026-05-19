using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ПрилТехно.Models;
using ПрилТехно.Services;

namespace ПрилТехно.ViewModels
{
    public partial class StepEditViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<EquipmentDto> _equipmentList = new();

        [ObservableProperty]
        private EquipmentDto? _selectedEquipment;

        [ObservableProperty]
        private TechStepDto? _step;

        [ObservableProperty]
        private bool _isNew = true;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event EventHandler? CloseRequest;

        public StepEditViewModel(ApiClient apiClient, IDialogService dialogService, TechStepDto? existing = null)
        {
            _apiClient = apiClient;
            _dialogService = dialogService;

            if (existing != null)
            {
                Step = existing;
                IsNew = false;
            }
            else
            {
                Step = new TechStepDto { IsMandatory = true };
            }

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));

            LoadEquipmentAsync().ConfigureAwait(false);
        }

        private async Task LoadEquipmentAsync()
        {
            var response = await _apiClient.GetAsync<List<EquipmentDto>>("/api/reference/equipment");
            if (response?.IsSuccess == true && response.Data != null)
            {
                EquipmentList = new ObservableCollection<EquipmentDto>(response.Data);
                if (Step!.EquipmentId.HasValue)
                    SelectedEquipment = EquipmentList.FirstOrDefault(e => e.Id == Step.EquipmentId);
            }
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Step!.StepName))
            {
                _dialogService.ShowMessage("Введите название шага", "Ошибка");
                return;
            }
            if (SelectedEquipment != null)
                Step.EquipmentId = SelectedEquipment.Id;
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}