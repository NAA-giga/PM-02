using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using прогОпер.Models;
using прогОпер.Repositories;
using прогОпер.Services;
using прогОпер.ViewModels;
using прогОпер.Views;

namespace прогОпер
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            // Конфигурация
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("ManufacturingDB");
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Строка подключения не найдена");

            services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
            services.AddScoped<IOperatorRepository, OperatorRepository>();
            services.AddScoped<IEventLogger, EventLogger>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // ViewModels
            services.AddTransient<ActiveBatchesViewModel>();
            services.AddTransient<BatchExecutionViewModel>();

            // Views
            services.AddTransient<ActiveBatchesView>();
            services.AddSingleton<MainWindow>();
            services.AddTransient<BatchExecutionView>();

            Services = services.BuildServiceProvider();

            // === ПРОПУСК ВХОДА ===
            // 1. Установим "фейкового" пользователя, если нужно
            var authService = Services.GetRequiredService<IAuthService>();
            // Предположим, в AuthService есть метод SetCurrentUser (или можно напрямую присвоить через рефлексию/интерфейс)
            // Если у вас нет такого метода, добавьте в IAuthService: void SetUser(UserProfile user);
            var mockUser = new UserProfile
            {
                Id = 1,
                Username = "operator",
                FullName = "Оператор (тестовый)",
                Role = "operator",
                Department = "Production"
            };
            // Если метод SetUser есть:
            // authService.SetUser(mockUser);
            // Если нет - можно временно закомментировать строки, которые используют authService в MainWindow

            // 2. Сразу показываем MainWindow
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}