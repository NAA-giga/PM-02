using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using ПрилТехно.Services;
using ПрилТехно.Views;

namespace ПрилТехно.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _userFullName = string.Empty;

        [ObservableProperty]
        private string _userRole = string.Empty;

        [ObservableProperty]
        private string? _userPhotoBase64;

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel(INavigationService navigationService, IAuthService authService)
        {
            _navigationService = navigationService;
            _authService = authService;

            if (_authService.CurrentUser != null)
            {
                UserFullName = _authService.CurrentUser.FullName;
                UserRole = _authService.CurrentUser.Role;
                UserPhotoBase64 = _authService.CurrentUser.PhotoBase64;
            }

            NavigateCommand = new RelayCommand<string>(Navigate);
            LogoutCommand = new RelayCommand(Logout);
        }

        private void Navigate(string? pageName)
        {
            switch (pageName)
            {
                case "Dashboard":
                    _navigationService.NavigateTo<DashboardView, DashboardViewModel>();
                    break;
                case "Products":
                    _navigationService.NavigateTo<ProductsView, ProductsViewModel>();
                    break;
                case "Recipes":
                    _navigationService.NavigateTo<RecipesView, RecipesViewModel>();
                    break;
                case "TechCards":
                    _navigationService.NavigateTo<TechCardsView, TechCardsViewModel>();
                    break;
                case "Orders":
                    _navigationService.NavigateTo<OrdersView, OrdersViewModel>();
                    break;
                case "Batches":
                    _navigationService.NavigateTo<BatchesView, BatchesViewModel>();
                    break;
                case "Extruder":
                    _navigationService.NavigateTo<ExtruderView, ExtruderViewModel>();
                    break;
                case "Events":
                    _navigationService.NavigateTo<EventsView, EventsViewModel>();
                    break;
                case "Reports":
                    _navigationService.NavigateTo<ReportsView, ReportsViewModel>();
                    break;
                default:
                    _navigationService.NavigateTo<DashboardView, DashboardViewModel>();
                    break;
            }
        }

        private void Logout()
        {
            // Подтверждение выхода (опционально)
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            // Очищаем данные авторизации
            _authService.Logout();

            // Создаём НОВОЕ окно входа через DI
            var loginView = App.Services.GetRequiredService<LoginView>();
            loginView.Show();

            // Закрываем текущее главное окно
            Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
        }
    }
}
