using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace ПрилТехно.Services
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
            System.Diagnostics.Debug.WriteLine($"Navigating to {typeof(TView)}");
            var viewModel = App.Services.GetRequiredService<TViewModel>();
            System.Diagnostics.Debug.WriteLine($"ViewModel obtained: {viewModel != null}");
            view.DataContext = viewModel;
            System.Diagnostics.Debug.WriteLine($"DataContext set: {view.DataContext}");
            _contentControl.Content = view;

        }
    }
}