using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ПрилТехно.Models;
using ПрилТехно.Services;

namespace ПрилТехно.ViewModels
{
    public partial class ProductEditViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly IDialogService _dialogService;
        private readonly bool _isEditMode;

        [ObservableProperty]
        private ProductDto _product = new();

        public event EventHandler? CloseRequest;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ProductEditViewModel(ApiClient apiClient, IDialogService dialogService, ProductDto? existing = null)
        {
            _apiClient = apiClient;
            _dialogService = dialogService;

            if (existing != null)
            {
                _isEditMode = true;
                Product = existing;
            }

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Product.Code) || string.IsNullOrWhiteSpace(Product.Name))
            {
                _dialogService.ShowMessage("Заполните код и наименование продукта", "Ошибка");
                return;
            }

            try
            {
                ApiResponse<object>? response;
                if (_isEditMode)
                {
                    response = await _apiClient.PutAsync<object>($"/api/reference/products/{Product.Id}", Product);
                }
                else
                {
                    response = await _apiClient.PostAsync<object>("/api/reference/products", Product);
                }

                if (response?.IsSuccess == true)
                {
                    CloseRequest?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _dialogService.ShowMessage(response?.ErrorMessage ?? "Ошибка сохранения", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
    }
}