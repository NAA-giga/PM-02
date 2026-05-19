    using LaboratoryApp.ViewModels;
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
    using ПрогЛабор.ViewModels;
    using ПрогЛабор.Views;

namespace ПрогЛабор.Views
    {
        /// <summary>
        /// Логика взаимодействия для QualityTestView.xaml
        /// </summary>
        public partial class QualityTestView : Window
        {
            public QualityTestView()
            {
                InitializeComponent();
            }
        public QualityTestView(QualityTestViewModel viewModel) : this()
        {
            DataContext = viewModel;
            viewModel.CloseRequest += (s, e) => DialogResult = true;
        }
    }
    }
