using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ПрилТехно.Models;
using ПрилТехно.Services;
using ПрилТехно.Views;

namespace ПрилТехно.ViewModels
{
    public partial class TechCardsViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<TechCardDto> _techCards = new();

        [ObservableProperty]
        private bool _isLoading;

        public ICommand LoadTechCardsCommand { get; }
        public ICommand CreateTechCardCommand { get; }
        public ICommand EditTechCardCommand { get; }
        public ICommand ApproveTechCardCommand { get; }
        public ICommand ArchiveTechCardCommand { get; }
        public ICommand RefreshCommand { get; }

        public TechCardsViewModel(ApiClient apiClient, IDialogService dialogService)
        {
            _apiClient = apiClient;
            _dialogService = dialogService;

            LoadTechCardsCommand = new AsyncRelayCommand(LoadTechCardsAsync);
            CreateTechCardCommand = new AsyncRelayCommand(CreateTechCardAsync);
            EditTechCardCommand = new AsyncRelayCommand<TechCardDto?>(EditTechCardAsync);
            ApproveTechCardCommand = new AsyncRelayCommand<TechCardDto?>(ApproveTechCardAsync);
            ArchiveTechCardCommand = new AsyncRelayCommand<TechCardDto?>(ArchiveTechCardAsync);
            RefreshCommand = new AsyncRelayCommand(LoadTechCardsAsync);
        }

        private async Task LoadTechCardsAsync()
        {
            IsLoading = true;
            try
            {
                var response = await _apiClient.GetAsync<List<TechCardDto>>("/api/techcards");
                if (response?.IsSuccess == true && response.Data != null)
                    TechCards = new ObservableCollection<TechCardDto>(response.Data);
                else
                    _dialogService.ShowMessage(response?.ErrorMessage ?? "Ошибка загрузки карт", "Ошибка");
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

        private async Task CreateTechCardAsync()
        {
            var editVm = new TechCardEditViewModel(_apiClient, _dialogService);
            var editWindow = new TechCardEditView(editVm);
            if (editWindow.ShowDialog() == true)
                await LoadTechCardsAsync();
        }

        private async Task EditTechCardAsync(TechCardDto? card)
        {
            if (card == null) return;
            var editVm = new TechCardEditViewModel(_apiClient, _dialogService, card.Id);
            var editWindow = new TechCardEditView(editVm);
            if (editWindow.ShowDialog() == true)
                await LoadTechCardsAsync();
        }

        private async Task ApproveTechCardAsync(TechCardDto? card)
        {
            if (card == null) return;
            if (!_dialogService.ShowConfirmation($"Утвердить карту '{card.Name}'?", "Подтверждение"))
                return;

            try
            {
                var response = await _apiClient.PostAsync<object>($"/api/techcards/{card.Id}/approve", null);
                if (response?.IsSuccess == true)
                    await LoadTechCardsAsync();
                else
                    _dialogService.ShowMessage(response?.ErrorMessage ?? "Ошибка утверждения", "Ошибка");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private async Task ArchiveTechCardAsync(TechCardDto? card)
        {
            if (card == null) return;
            if (!_dialogService.ShowConfirmation($"Архивировать карту '{card.Name}'?", "Подтверждение"))
                return;

            try
            {
                var response = await _apiClient.PostAsync<object>($"/api/techcards/{card.Id}/archive", null);
                if (response?.IsSuccess == true)
                    await LoadTechCardsAsync();
                else
                    _dialogService.ShowMessage(response?.ErrorMessage ?? "Ошибка архивации", "Ошибка");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
    }
}