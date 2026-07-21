using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using KHAONPOS.Data;
using KHAONPOS.Services;
using KHAONPOS.ViewModels;
using KHAONPOS.Views;

namespace KHAONPOS;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // Register DbContext
                services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);

                // Register Services
                services.AddTransient<IOrderService, OrderService>();
                services.AddTransient<IInventoryService, InventoryService>();
                services.AddTransient<IReportingService, ReportingService>();

                // Register ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddTransient<LoginViewModel>();
                services.AddTransient<CashierViewModel>();
                services.AddTransient<KitchenViewModel>();
                services.AddTransient<AdminDashboardViewModel>();
                services.AddTransient<AdminOverviewViewModel>();
                services.AddTransient<AdminAnalyticsViewModel>();
                services.AddTransient<AdminReportsViewModel>();
                services.AddTransient<AdminUsersViewModel>();
                services.AddTransient<AdminInventoryViewModel>();
                services.AddTransient<AdminCategoryViewModel>();

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
        DispatcherUnhandledException += (s, args) => File.WriteAllText("crash.log", args.Exception.ToString());

        try
        {
            _host.Start();

            // Ensure database is created and seeded
            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
                DbSeeder.SeedOrderItemsIfEmpty(dbContext);
                DbSeeder.MigrateLocalImagesToDatabase(dbContext);
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
