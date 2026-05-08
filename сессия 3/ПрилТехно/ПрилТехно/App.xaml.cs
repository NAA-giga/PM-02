using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ПрилТехно.Services;
using ПрилТехно.ViewModels;
using ПрилТехно.Views;

namespace ПрилТехно
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Регистрация сервисов
                    services.AddHttpClient<ApiService>(client =>
                    {
                        client.BaseAddress = new Uri("http://localhost:5000/api/");
                    });
                    services.AddSingleton<CaptchaService>();

                    // Регистрация окон
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<MainWindow>();

                    // Регистрация ViewModels (если используете MVVM)
                    services.AddTransient<RecipesViewModel>();
                    services.AddTransient<ProductsViewModel>();
                    // ... остальные ViewModels

                    // Регистрация Views (если нужно)
                    services.AddTransient<RecipesView>();
                    services.AddTransient<ProductsView>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            var login = _host.Services.GetRequiredService<LoginWindow>();
            if (login.ShowDialog() == true)
            {
                var main = _host.Services.GetRequiredService<MainWindow>();
                main.Show();
            }
            else
            {
                Shutdown();
            }

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
