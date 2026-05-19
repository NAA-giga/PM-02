// App.xaml.cs
using LaboratoryApp.Repositories;
using LaboratoryApp.Services;
using LaboratoryApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Windows;
using ПрогЛабор.Services;
using ПрогЛабор.ViewModels;
using ПрогЛабор.Views;

namespace ПрогЛабор
{
    public partial class App : Application
    {
        public static IServiceProvider? Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Фабрика подключений (синглтон)
            services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

            // Репозиторий (scoped или transient – для WPF подойдёт transient)
            services.AddScoped<ILabRepository, LabRepository>();

            // Сервисы
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IEventLogger, EventLogger>();
            services.AddSingleton<IProtocolGenerator, ProtocolGenerator>();
            services.AddSingleton<IAuthService, AuthService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<RawMaterialBatchesViewModel>();
            services.AddTransient<ProductBatchesViewModel>();
            services.AddTransient<RawMaterialTestViewModel>();
            services.AddTransient<QualityTestViewModel>();
            services.AddTransient<LabDecisionViewModel>();

            // Views (окна и контролы)
            services.AddTransient<MainWindow>();
            services.AddTransient<RawMaterialBatchesView>();
            services.AddTransient<ProductBatchesView>();
            services.AddTransient<RawMaterialTestView>();
            services.AddTransient<QualityTestView>();
            services.AddTransient<LabDecisionView>();
        }
    }
}