using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using KHAONPOS.Data.Entities;
using KHAONPOS.Services;

namespace KHAONPOS.ViewModels;

public class AdminReportsViewModel : BaseViewModel
{
    private readonly IReportingService _reportingService;

    public ObservableCollection<Order> RecentOrders { get; } = [];

    public ICommand RefreshCommand { get; }

    public AdminReportsViewModel(IReportingService reportingService)
    {
        _reportingService = reportingService;
        RefreshCommand = new RelayCommand(async _ => await LoadReportsAsync());

        _ = LoadReportsAsync();
    }


    private async Task LoadReportsAsync()
    {
        RecentOrders.Clear();
        var orders = await _reportingService.GetRecentOrdersAsync(50);
        foreach (var order in orders)
        {
            RecentOrders.Add(order);
        }
    }
}
