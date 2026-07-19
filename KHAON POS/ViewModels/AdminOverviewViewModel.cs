using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using RestaurantPOS.Services;
using RestaurantPOS.Data.Entities;
using RestaurantPOS.Data.Models;

namespace RestaurantPOS.ViewModels;

public class AdminOverviewViewModel : BaseViewModel
{
    private readonly IReportingService _reportingService;

    private decimal _totalSales;
    public decimal TotalSales
    {
        get => _totalSales;
        set => SetProperty(ref _totalSales, value);
    }

    private int _orderCount;
    public int OrderCount
    {
        get => _orderCount;
        set => SetProperty(ref _orderCount, value);
    }

    private decimal _averageOrderValue;
    public decimal AverageOrderValue
    {
        get => _averageOrderValue;
        set => SetProperty(ref _averageOrderValue, value);
    }

    private int _activeCustomers;
    public int ActiveCustomers
    {
        get => _activeCustomers;
        set => SetProperty(ref _activeCustomers, value);
    }

    public ObservableCollection<ISeries> SalesSeries { get; set; }
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; set; }

    public ObservableCollection<Order> RecentOrders { get; } = new();
    public ObservableCollection<TopItemDTO> TopItems { get; } = new();

    public ICommand RefreshDataCommand { get; }

    public AdminOverviewViewModel(IReportingService reportingService)
    {
        _reportingService = reportingService;

        RefreshDataCommand = new RelayCommand(async _ => await LoadDashboardData());

        var mainColor = SKColor.Parse("#66BD76");

        // LiveCharts2 configuration for a simple area chart
        SalesSeries = new ObservableCollection<ISeries>
        {
            new LineSeries<decimal>
            {
                Values = new[] { 12m, 25m, 18m, 35m, 40m, 55m, 45m }, // Mock data (thousands)
                Name = "Revenue",
                GeometryFill = null,
                GeometryStroke = null,
                Fill = new SolidColorPaint(mainColor.WithAlpha(50)),
                Stroke = new SolidColorPaint(mainColor) { StrokeThickness = 3 },
                LineSmoothness = 0.5
            }
        };

        XAxes = new[]
        {
            new Axis
            {
                Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul" },
                LabelsPaint = new SolidColorPaint(SKColors.Gray)
            }
        };

        YAxes = new[]
        {
            new Axis
            {
                Labeler = value => value.ToString("C0") + "k",
                LabelsPaint = new SolidColorPaint(SKColors.Gray)
            }
        };

        _ = LoadDashboardData();
    }

    private async Task LoadDashboardData()
    {
        TotalSales = await _reportingService.GetTotalSalesAsync(DateTime.Today);
        OrderCount = await _reportingService.GetOrderCountAsync(DateTime.Today);
        
        AverageOrderValue = OrderCount > 0 ? TotalSales / OrderCount : 0;
        
        // Mock data for ActiveCustomers
        ActiveCustomers = 854; 

        var recentOrders = await _reportingService.GetRecentOrdersAsync(5);
        RecentOrders.Clear();
        foreach(var order in recentOrders) RecentOrders.Add(order);

        var topItems = await _reportingService.GetTopItemsAsync(3);
        TopItems.Clear();
        foreach(var item in topItems) TopItems.Add(item);
    }
}
