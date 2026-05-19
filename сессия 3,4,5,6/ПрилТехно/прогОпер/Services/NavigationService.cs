using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace прогОпер.Services
{
    public class NavigationService : INavigationService
    {
        private ContentControl? _contentControl;

        public void Initialize(ContentControl contentControl)
        {
            _contentControl = contentControl;
        }

        // Метод для автоматического создания View и ViewModel
        public void NavigateTo<TView, TViewModel>()
            where TView : UserControl, new()
            where TViewModel : class
        {
            if (_contentControl == null)
                throw new System.InvalidOperationException("NavigationService не инициализирован");

            var view = new TView();
            var viewModel = App.Services.GetRequiredService<TViewModel>();
            view.DataContext = viewModel;
            _contentControl.Content = view;
        }

        // Метод для перехода с уже готовым View
        public void NavigateTo<TView>(TView view) where TView : UserControl
        {
            if (_contentControl == null)
                throw new System.InvalidOperationException("NavigationService не инициализирован");
            _contentControl.Content = view;
        }
    }
}