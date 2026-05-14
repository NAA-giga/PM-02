using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Windows;
using System.Windows.Navigation;
using ПрилТехно.Services;
using ПрилТехно.ViewModels;
using ПрилТехно.Views;
using INavigationService = ПрилТехно.Services.INavigationService;
using NavigationService = ПрилТехно.Services.NavigationService;

namespace ПрилТехно
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                var services = new ServiceCollection();

                // HTTP клиент с базовым адресом API
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("http://localhost:5167/"),
                    Timeout = TimeSpan.FromSeconds(30)
                };
                services.AddSingleton(httpClient);          // один HttpClient на всё приложение
                services.AddSingleton<ApiClient>();

                // Регистрация сервисов
                services.AddSingleton<IAuthService, AuthService>();
                services.AddSingleton<ICaptchaService, CaptchaService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IDialogService, DialogService>();

                // ViewModels
                services.AddTransient<LoginViewModel>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<ProductsViewModel>();
                services.AddTransient<RecipesViewModel>();
                services.AddTransient<TechCardsViewModel>();
                services.AddTransient<OrdersViewModel>();
                services.AddTransient<BatchesViewModel>();
                services.AddTransient<ExtruderViewModel>();
                services.AddTransient<EventsViewModel>();
                services.AddTransient<ReportsViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ProductsViewModel>();
                services.AddTransient<ProductEditViewModel>();
                services.AddTransient<ComponentEditViewModel>();
                services.AddTransient<RecipeEditViewModel>();
                services.AddTransient<TechCardEditViewModel>();
                services.AddTransient<TechCardEditView>();
                services.AddTransient<StepEditViewModel>();

                // Views (singleton для окон)
                services.AddTransient<LoginView>();
                services.AddTransient<MainWindow>();
                services.AddTransient<DashboardView>();
                services.AddTransient<ProductsView>();
                services.AddTransient<ProductEditView>();
                services.AddTransient<RecipeEditView>();
                services.AddTransient<TechCardsView>();
                services.AddTransient<StepEditView>();
                services.AddTransient<ReportsView>();

                services.AddTransient<ComponentEditView>();

                Services = services.BuildServiceProvider();

                var loginView = Services.GetRequiredService<LoginView>();
                var mainWindow = Services.GetRequiredService<MainWindow>();

                // Подписка на закрытие окна входа
                loginView.Closed += (s, args) =>
                {
                    if (Services.GetRequiredService<IAuthService>().IsAuthenticated)
                    {
                        var mainVm = Services.GetRequiredService<MainViewModel>();
                        mainWindow.DataContext = mainVm;
                        mainWindow.Show();
                    }
                    else
                    {
                        Shutdown(); // если не авторизован, выходим
                    }
                };
                loginView.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске:\n{ex.Message}\n\n{ex.StackTrace}",
                "Критическая ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }
    }

}
