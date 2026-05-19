using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using System.Windows.Navigation;
using прогОпер.Models;
using прогОпер.Repositories;
using прогОпер.Services;
using прогОпер.Views;

namespace прогОпер.ViewModels
{
    public partial class ActiveBatchesViewModel : ObservableObject
    {
        private readonly IOperatorRepository _repository;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private ObservableCollection<ActiveBatchDto> _batches = new();

        [ObservableProperty]
        private ActiveBatchDto? _selectedBatch;

        [ObservableProperty]
        private bool _isLoading;

        public ICommand LoadBatchesCommand { get; }
        public ICommand SelectBatchCommand { get; }
        public ICommand RefreshCommand { get; }

        public ActiveBatchesViewModel(
            IOperatorRepository repository,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _repository = repository;
            _dialogService = dialogService;
            _navigationService = navigationService;

            LoadBatchesCommand = new AsyncRelayCommand(LoadBatchesAsync);
            SelectBatchCommand = new AsyncRelayCommand(SelectBatchAsync, () => SelectedBatch != null);
            RefreshCommand = new AsyncRelayCommand(LoadBatchesAsync);
        }

        private async Task LoadBatchesAsync()
        {
            IsLoading = true;
            try
            {
                var batches = await _repository.GetActiveBatchesAsync();
                Batches = new ObservableCollection<ActiveBatchDto>(batches);
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

        private async Task SelectBatchAsync()
        {
            if (SelectedBatch == null) return;

            var executionVm = App.Services.GetRequiredService<BatchExecutionViewModel>();
            await executionVm.InitializeAsync(SelectedBatch.Id);
            var executionView = new BatchExecutionView();
            executionView.SetViewModel(executionVm);
            _navigationService.NavigateTo<BatchExecutionView>(executionView);
        }
    }
}
