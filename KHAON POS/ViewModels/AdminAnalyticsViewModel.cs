using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels;

public class AdminAnalyticsViewModel : BaseViewModel
{
    private readonly IReportingService _reportingService;

    public ObservableCollection<ISeries> CategorySalesSeries { get; set; }

    public AdminAnalyticsViewModel(IReportingService reportingService)
    {
        _reportingService = reportingService;

        // Mock data for a pie chart or bar chart
        CategorySalesSeries = new ObservableCollection<ISeries>
        {
            new PieSeries<double> { Values = new double[] { 45 }, Name = "Main Course" },
            new PieSeries<double> { Values = new double[] { 25 }, Name = "Beverages" },
            new PieSeries<double> { Values = new double[] { 20 }, Name = "Appetizers" },
            new PieSeries<double> { Values = new double[] { 10 }, Name = "Desserts" }
        };
    }
}
