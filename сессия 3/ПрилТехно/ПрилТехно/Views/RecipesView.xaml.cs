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
using ПрилТехно.Services;
using ПрилТехно.ViewModels;

namespace ПрилТехно.Views
{
    /// <summary>
    /// Логика взаимодействия для RecipesView.xaml
    /// </summary>
    public partial class RecipesView : Window
    {
        public RecipesView(ApiService api)
        {
            InitializeComponent();
            DataContext = new RecipesViewModel(api);

            // Автоматическая загрузка данных при появлении контрола
            Loaded += async (s, e) =>
            {
                if (DataContext is RecipesViewModel vm && vm.LoadDataCommand.CanExecute(null))
                    await vm.LoadDataCommand.ExecuteAsync(null);
            };
        }
    }
}
