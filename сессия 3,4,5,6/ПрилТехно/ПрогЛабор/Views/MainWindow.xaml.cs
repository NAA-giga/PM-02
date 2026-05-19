using LaboratoryApp.ViewModels;
using System.Windows;
using System.Windows.Navigation;
using ПрогЛабор.Services;
using ПрогЛабор.ViewModels;

namespace ПрогЛабор.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel, INavigationService navigationService)
        {
            InitializeComponent();
            DataContext = viewModel;
            navigationService.Initialize(MainContent);
            navigationService.NavigateTo<RawMaterialBatchesView, RawMaterialBatchesViewModel>();
        }
    }
}