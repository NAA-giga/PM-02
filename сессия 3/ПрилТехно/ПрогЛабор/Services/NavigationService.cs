using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace ПрогЛабор.Services
{
    public class NavigationService : INavigationService
    {
        private ContentControl? _contentControl;

        public void Initialize(ContentControl contentControl)
        {
            _contentControl = contentControl;
        }

        public void NavigateTo<TView, TViewModel>()
            where TView : UserControl, new()
            where TViewModel : class
        {
            if (_contentControl == null)
                throw new InvalidOperationException("NavigationService не инициализирован");

            var view = new TView();
            var viewModel = App.Services.GetRequiredService<TViewModel>();
            view.DataContext = viewModel;
            _contentControl.Content = view;
        }
    }
}