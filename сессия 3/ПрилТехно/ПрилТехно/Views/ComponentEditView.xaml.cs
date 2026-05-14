using System.Windows;
using ПрилТехно.ViewModels;

namespace ПрилТехно.Views
{
    public partial class ComponentEditView : Window
    {
        public ComponentEditView(ComponentEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequest += (s, e) => DialogResult = true;
        }
    }
}