using System.Windows.Controls;

namespace ПрилТехно.Services
{
    public interface INavigationService
    {
        void Initialize(ContentControl contentControl);
        void NavigateTo<TView, TViewModel>()
            where TView : UserControl, new()
            where TViewModel : class;
    }
}