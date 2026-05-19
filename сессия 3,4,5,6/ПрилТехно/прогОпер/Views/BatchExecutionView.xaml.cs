using System.Windows.Controls;
using прогОпер.ViewModels;

namespace прогОпер.Views
{
    public partial class BatchExecutionView : UserControl
    {
        public BatchExecutionView()
        {
            InitializeComponent();
        }

        // Метод для установки ViewModel (используется навигацией)
        public void SetViewModel(BatchExecutionViewModel viewModel)
        {
            DataContext = viewModel;
        }
    }
}