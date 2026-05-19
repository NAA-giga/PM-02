using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace прогОпер.Services
{
    public interface INavigationService
    {
        void Initialize(ContentControl contentControl);

        // Старый метод для создания View и ViewModel автоматически
        void NavigateTo<TView, TViewModel>()
            where TView : UserControl, new()
            where TViewModel : class;

        // Новый метод для передачи уже созданного View (с готовой ViewModel)
        void NavigateTo<TView>(TView view) where TView : UserControl;
    }
}
