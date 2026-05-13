using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ПрилТехно.Services
{
    public class DialogService : IDialogService
    {
        public void ShowMessage(string message, string title = "Информация")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool ShowConfirmation(string message, string title = "Подтверждение")
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
