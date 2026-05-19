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
using прогОпер.Services;

namespace прогОпер.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        private readonly IAuthService _authService;

        public LoginView(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var success = await _authService.LoginAsync(UsernameBox.Text, PasswordBox.Password);
            if (success)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                ErrorText.Text = "Неверные учётные данные или недостаточно прав (требуется роль operator)";
            }
        }
    }
}
