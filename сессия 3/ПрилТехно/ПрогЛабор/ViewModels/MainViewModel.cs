using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaboratoryApp.ViewModels;
using System.Windows.Input;
using ПрогЛабор.Services;
using ПрогЛабор.Views;

namespace ПрогЛабор.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _userFullName = string.Empty;

        [ObservableProperty]
        private string _userRole = string.Empty;

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
            }

            NavigateCommand = new RelayCommand<string>(Navigate);
            LogoutCommand = new RelayCommand(Logout);
        }

        private void Navigate(string? pageName)
        {
            switch (pageName)
            {
                case "RawMaterial":
                    _navigationService.NavigateTo<RawMaterialBatchesView, RawMaterialBatchesViewModel>();
                    break;
                case "Product":
                    _navigationService.NavigateTo<ProductBatchesView, ProductBatchesViewModel>();
                    break;
                default:
                    _navigationService.NavigateTo<RawMaterialBatchesView, RawMaterialBatchesViewModel>();
                    break;
            }
        }

        private void Logout()
        {
            _authService.Logout();
            var loginView = App.Services.GetRequiredService<LoginView>();
            loginView.Show();
            System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
        }
    }
}