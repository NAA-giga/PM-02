using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ПрилТехно.Services;
using ПрилТехно.ViewModels;
using ПрилТехно.Views;

namespace ПрилТехно
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel, INavigationService navigationService)
        {
            InitializeComponent();
            DataContext = viewModel;
            navigationService.Initialize(MainContent);
            // По умолчанию открываем Dashboard
            navigationService.NavigateTo<DashboardView, DashboardViewModel>();

        }
    }
}