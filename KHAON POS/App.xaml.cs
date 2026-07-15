using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestaurantPOS.Data;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels;
using RestaurantPOS.Views;

namespace RestaurantPOS;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register DbContext
                services.AddDbContext<AppDbContext>();

                // Register Services
                services.AddSingleton<IOrderService, OrderService>();
                services.AddSingleton<IInventoryService, InventoryService>();
                services.AddSingleton<IReportingService, ReportingService>();
                services.AddSingleton<IBarcodeService, BarcodeService>();

                // Register ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddTransient<LoginViewModel>();
                services.AddTransient<PosViewModel>();
                services.AddTransient<KitchenViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<AdminOverviewViewModel>();
                services.AddTransient<AdminAnalyticsViewModel>();
                services.AddTransient<AdminReportsViewModel>();
                services.AddTransient<AdminUsersViewModel>();
                services.AddTransient<AdminInventoryViewModel>();

                // Register MainWindow
                services.AddSingleton<MainWindow>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
            })
            .Build();
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        try
        {
            _host.Start();

            // Ensure database is created and seeded
            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
            }

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Application crashed on startup:\n\n{ex.Message}\n\n{ex.StackTrace}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Current.Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host.StopAsync().Wait();
        _host.Dispose();
        base.OnExit(e);
    }
}
