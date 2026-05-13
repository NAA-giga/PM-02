using CommunityToolkit.Mvvm.ComponentModel;
using ПрилТехно.Services;

namespace ПрилТехно.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        // Ручные свойства (без [ObservableProperty])
        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        private string _role = string.Empty;
        public string Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        private int _activeProductsCount = 0;
        public int ActiveProductsCount
        {
            get => _activeProductsCount;
            set => SetProperty(ref _activeProductsCount, value);
        }

        private int _activeBatchesCount = 0;
        public int ActiveBatchesCount
        {
            get => _activeBatchesCount;
            set => SetProperty(ref _activeBatchesCount, value);
        }

        public DashboardViewModel(IAuthService authService)
        {
            _authService = authService;
            if (_authService.CurrentUser != null)
            {
                FullName = _authService.CurrentUser.FullName;
                Role = _authService.CurrentUser.Role;
            }
            // Здесь будут загрузки реальных данных через API
        }
    }
}