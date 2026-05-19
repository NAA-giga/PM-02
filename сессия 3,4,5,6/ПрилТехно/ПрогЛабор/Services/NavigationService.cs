using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace ПрогЛабор.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private ContentControl? _contentControl;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Initialize(ContentControl contentControl) => _contentControl = contentControl;

        public void NavigateTo<TView, TViewModel>()
            where TView : UserControl, new()
            where TViewModel : class
        {
            if (_contentControl == null) throw new InvalidOperationException();
            var view = new TView();
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            view.DataContext = viewModel;
            _contentControl.Content = view;
        }
    }
}