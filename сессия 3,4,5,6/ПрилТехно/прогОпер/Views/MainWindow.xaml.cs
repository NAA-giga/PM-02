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
using прогОпер.ViewModels;
using прогОпер.Views;
using прогОпер.Services;

namespace прогОпер
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly INavigationService _navigationService;

        public MainWindow(INavigationService navigationService, ActiveBatchesViewModel viewModel)
        {
            InitializeComponent();
            _navigationService = navigationService;
            _navigationService.Initialize(MainContent);
            _navigationService.NavigateTo<ActiveBatchesView, ActiveBatchesViewModel>();
        }
    }
}