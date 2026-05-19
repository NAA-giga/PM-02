using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ПрилТехно.ViewModels;

namespace ПрилТехно.Views
{
    /// <summary>
    /// Логика взаимодействия для StepEditView.xaml
    /// </summary>
    public partial class StepEditView : Window
    {
        public StepEditView(StepEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequest += (s, e) => DialogResult = true;
        }
    }
}
