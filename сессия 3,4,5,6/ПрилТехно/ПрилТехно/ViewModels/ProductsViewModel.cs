using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using ПрилТехно.Models;
using ПрилТехно.Services;
using ПрилТехно.Views;

namespace ПрилТехно.ViewModels
{
    public partial class ProductsViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<ProductDto> _products = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _searchText = string.Empty;

        public ICommand LoadProductsCommand { get; }
        public ICommand CreateProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand ArchiveProductCommand { get; }
        public ICommand RefreshCommand { get; }

        public ProductsViewModel(ApiClient apiClient, IDialogService dialogService)
        {
            _apiClient = apiClient;
            _dialogService = dialogService;

            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
            CreateProductCommand = new AsyncRelayCommand(CreateProductAsync);
            EditProductCommand = new AsyncRelayCommand<ProductDto?>(EditProductAsync);
            ArchiveProductCommand = new AsyncRelayCommand<ProductDto?>(ArchiveProductAsync);
            RefreshCommand = new AsyncRelayCommand(LoadProductsAsync);
        }

        private async Task LoadProductsAsync()
        {
            IsLoading = true;
            try
            {
                var response = await _apiClient.GetAsync<List<ProductDto>>("/api/reference/products");
                if (response?.IsSuccess == true && response.Data != null)
                {
                    Products = new ObservableCollection<ProductDto>(response.Data);
                }
                else
                {
                    _dialogService.ShowMessage(response?.ErrorMessage ?? "Ошибка загрузки продуктов", "Ошибка");
                }
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

        private async Task CreateProductAsync()
        {
            // Открываем окно создания продукта
            var editVm = new ProductEditViewModel(_apiClient, _dialogService);
            var editWindow = new ProductEditView(editVm);
            if (editWindow.ShowDialog() == true)
            {
                await LoadProductsAsync(); // обновить список
            }
        }

        private async Task EditProductAsync(ProductDto? product)
        {
            if (product == null) return;
            var editVm = new ProductEditViewModel(_apiClient, _dialogService, product);
            var editWindow = new ProductEditView(editVm);
            if (editWindow.ShowDialog() == true)
            {
                await LoadProductsAsync();
            }
        }

        private async Task ArchiveProductAsync(ProductDto? product)
        {
            if (product == null) return;
            if (!_dialogService.ShowConfirmation($"Архивировать продукт {product.Name}?", "Подтверждение"))
                return;

            try
            {
                var response = await _apiClient.DeleteAsync<object>($"/api/reference/products/{product.Id}");
                if (response?.IsSuccess == true)
                {
                    await LoadProductsAsync();
                }
                else
                {
                    _dialogService.ShowMessage(response?.ErrorMessage ?? "Ошибка архивирования", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
    }
}
