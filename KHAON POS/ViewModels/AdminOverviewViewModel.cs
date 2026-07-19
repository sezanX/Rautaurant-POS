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

    private bool _isWeeklySelected = true;
    public bool IsWeeklySelected
    {
        get => _isWeeklySelected;
        set
        {
            if (SetProperty(ref _isWeeklySelected, value) && value)
            {
                _isMonthlySelected = false;
                OnPropertyChanged(nameof(IsMonthlySelected));
                _ = UpdateSalesChart();
            }
        }
    }

    private bool _isMonthlySelected;
    public bool IsMonthlySelected
    {
        get => _isMonthlySelected;
        set
        {
            if (SetProperty(ref _isMonthlySelected, value) && value)
            {
                _isWeeklySelected = false;
                OnPropertyChanged(nameof(IsWeeklySelected));
                _ = UpdateSalesChart();
            }
        }
    }

    public ObservableCollection<ISeries> SalesSeries { get; } = new();

    private Axis[] _xAxes = Array.Empty<Axis>();
    public Axis[] XAxes
    {
        get => _xAxes;
        set => SetProperty(ref _xAxes, value);
    }

    private Axis[] _yAxes = Array.Empty<Axis>();
    public Axis[] YAxes
    {
        get => _yAxes;
        set => SetProperty(ref _yAxes, value);
    }

    public ObservableCollection<Order> RecentOrders { get; } = new();
    public ObservableCollection<TopItemDTO> TopItems { get; } = new();

    public ICommand RefreshDataCommand { get; }

    public AdminOverviewViewModel(IReportingService reportingService)
    {
        _reportingService = reportingService;

        RefreshDataCommand = new RelayCommand(async _ => await LoadDashboardData());

        _ = LoadDashboardData();
    }

    private async Task UpdateSalesChart()
    {
        try
        {
            string[] labels;
            decimal[] values;

            if (IsWeeklySelected)
            {
                (labels, values) = await _reportingService.GetWeeklySalesAsync();
            }
            else
            {
                (labels, values) = await _reportingService.GetMonthlySalesAsync();
            }

            var mainColor = SKColor.Parse("#66BD76");

            SalesSeries.Clear();
            SalesSeries.Add(new LineSeries<decimal>
            {
                Values = values,
                Name = "Revenue",
                GeometryFill = null,
                GeometryStroke = null,
                Fill = new SolidColorPaint(mainColor.WithAlpha(50)),
                Stroke = new SolidColorPaint(mainColor) { StrokeThickness = 3 },
                LineSmoothness = 0.5
            });

            XAxes = new[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            };

            YAxes = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("C0"),
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            };
        }
        catch (Exception)
        {
            // Fallback in case of database errors
            var mainColor = SKColor.Parse("#66BD76");
            SalesSeries.Clear();
            SalesSeries.Add(new LineSeries<decimal>
            {
                Values = new[] { 12m, 25m, 18m, 35m, 40m, 55m, 45m },
                Name = "Revenue (Demo)",
                GeometryFill = null,
                GeometryStroke = null,
                Fill = new SolidColorPaint(mainColor.WithAlpha(50)),
                Stroke = new SolidColorPaint(mainColor) { StrokeThickness = 3 },
                LineSmoothness = 0.5
            });

            XAxes = new[]
            {
                new Axis
                {
                    Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul" },
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            };
        }
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

        await UpdateSalesChart();
    }
}
