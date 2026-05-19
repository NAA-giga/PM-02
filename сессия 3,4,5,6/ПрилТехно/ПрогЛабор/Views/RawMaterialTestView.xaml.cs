using LaboratoryApp.ViewModels;
using System.Windows;
using ПрогЛабор.ViewModels;

namespace ПрогЛабор.Views
{
    public partial class RawMaterialTestView : Window
    {
        public RawMaterialTestView(RawMaterialTestViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequest += (s, e) => DialogResult = true;
        }
    }
}