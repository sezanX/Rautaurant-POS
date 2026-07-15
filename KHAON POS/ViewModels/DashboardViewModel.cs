using System;
using System.Windows.Input;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private BaseViewModel _currentAdminViewModel;
    public BaseViewModel CurrentAdminViewModel
    {
        get => _currentAdminViewModel;
        set => SetProperty(ref _currentAdminViewModel, value);
    }

    private string _activeTab = "Overview";
    public string ActiveTab
    {
        get => _activeTab;
        set => SetProperty(ref _activeTab, value);
    }

    public ICommand NavigateAdminCommand { get; }

    private readonly IServiceProvider _serviceProvider;

    public DashboardViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        NavigateAdminCommand = new RelayCommand<string>(Navigate);

        // Default view
        Navigate("Overview");
    }

    private void Navigate(string? viewName)
    {
        if (string.IsNullOrEmpty(viewName)) return;

        ActiveTab = viewName;

        switch (viewName)
        {
            case "Overview":
                CurrentAdminViewModel = (BaseViewModel)_serviceProvider.GetService(typeof(AdminOverviewViewModel))!;
                break;
            case "Analytics":
                CurrentAdminViewModel = (BaseViewModel)_serviceProvider.GetService(typeof(AdminAnalyticsViewModel))!;
                break;
            case "Reports":
                CurrentAdminViewModel = (BaseViewModel)_serviceProvider.GetService(typeof(AdminReportsViewModel))!;
                break;
            case "Users":
                CurrentAdminViewModel = (BaseViewModel)_serviceProvider.GetService(typeof(AdminUsersViewModel))!;
                break;
            case "Inventory":
                CurrentAdminViewModel = (BaseViewModel)_serviceProvider.GetService(typeof(AdminInventoryViewModel))!;
                break;
        }
    }
}
