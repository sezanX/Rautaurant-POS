using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using KHAONPOS.Services;
using KHAONPOS.Data.Entities;
using KHAONPOS.Data.Models;

namespace KHAONPOS.ViewModels;

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

    private static readonly decimal[] DemoRevenueValues = [12m, 25m, 18m, 35m, 40m, 55m, 45m];
    private static readonly string[] DemoMonthLabels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul"];

    public ObservableCollection<string> PeriodOptions { get; } = ["Today", "This Week", "This Month", "All Time"];

    private string _selectedPeriod = "Today";
    public string SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (SetProperty(ref _selectedPeriod, value))
            {
                _ = LoadDashboardData();
            }
        }
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

    public ObservableCollection<ISeries> SalesSeries { get; } = [];

    private Axis[] _xAxes = [];
    public Axis[] XAxes
    {
        get => _xAxes;
        set => SetProperty(ref _xAxes, value);
    }

    private Axis[] _yAxes = [];
    public Axis[] YAxes
    {
        get => _yAxes;
        set => SetProperty(ref _yAxes, value);
    }

    public ObservableCollection<Order> RecentOrders { get; } = [];
    public ObservableCollection<TopItemDTO> TopItems { get; } = [];

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

            XAxes =
            [
                new Axis
                {
                    Labels = labels,
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            ];

            YAxes =
            [
                new Axis
                {
                    Labeler = value => value.ToString("C0"),
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            ];
        }
        catch (Exception)
        {
            // Fallback in case of database errors
            var mainColor = SKColor.Parse("#66BD76");
            SalesSeries.Clear();
            SalesSeries.Add(new LineSeries<decimal>
            {
                Values = DemoRevenueValues,
                Name = "Revenue (Demo)",
                GeometryFill = null,
                GeometryStroke = null,
                Fill = new SolidColorPaint(mainColor.WithAlpha(50)),
                Stroke = new SolidColorPaint(mainColor) { StrokeThickness = 3 },
                LineSmoothness = 0.5
            });

            XAxes =
            [
                new Axis
                {
                    Labels = DemoMonthLabels,
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            ];
        }
    }

    private async Task LoadDashboardData()
    {
        DateTime? startDate = null;
        DateTime? endDate = null;
        var now = DateTime.Now;

        if (SelectedPeriod == "Today")
        {
            startDate = DateTime.Today;
            endDate = DateTime.Today.AddDays(1).AddTicks(-1);
        }
        else if (SelectedPeriod == "This Week")
        {
            var diff = (int)now.DayOfWeek == 0 ? 6 : (int)now.DayOfWeek - 1;
            startDate = DateTime.Today.AddDays(-diff);
        }
        else if (SelectedPeriod == "This Month")
        {
            startDate = new DateTime(now.Year, now.Month, 1);
        }
        else // "All Time"
        {
            startDate = null;
            endDate = null;
        }

        TotalSales = await _reportingService.GetTotalSalesAsync(startDate, endDate);
        OrderCount = await _reportingService.GetOrderCountAsync(startDate, endDate);

        // Fallback to All Time if Today has no orders or sales yet
        if (SelectedPeriod == "Today" && OrderCount == 0 && TotalSales == 0m)
        {
            var allTimeSales = await _reportingService.GetTotalSalesAsync(null, null);
            var allTimeCount = await _reportingService.GetOrderCountAsync(null, null);
            if (allTimeSales > 0m || allTimeCount > 0)
            {
                _selectedPeriod = "All Time";
                OnPropertyChanged(nameof(SelectedPeriod));
                startDate = null;
                endDate = null;
                TotalSales = allTimeSales;
                OrderCount = allTimeCount;
            }
        }

        AverageOrderValue = OrderCount > 0 ? TotalSales / OrderCount : 0m;
        ActiveCustomers = await _reportingService.GetActiveCustomersCountAsync(startDate, endDate);

        var recentOrders = await _reportingService.GetRecentOrdersAsync(5);
        RecentOrders.Clear();
        foreach (var order in recentOrders) RecentOrders.Add(order);

        var topItems = await _reportingService.GetTopItemsAsync(3);
        TopItems.Clear();
        foreach (var item in topItems) TopItems.Add(item);

        await UpdateSalesChart();
    }
}
