using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using KHAONPOS.Services;

namespace KHAONPOS.ViewModels;

public class AdminAnalyticsViewModel : BaseViewModel
{
    private readonly IReportingService _reportingService;

    public ObservableCollection<ISeries> CategorySalesSeries { get; } = [];

    public AdminAnalyticsViewModel(IReportingService reportingService)
    {
        _reportingService = reportingService;
        _ = LoadAnalyticsData();
    }

    private async Task LoadAnalyticsData()
    {
        try
        {
            var categorySales = await _reportingService.GetCategorySalesAsync();
            CategorySalesSeries.Clear();

            foreach (var kvp in categorySales)
            {
                CategorySalesSeries.Add(new PieSeries<double>
                {
                    Values = [kvp.Value],
                    Name = kvp.Key
                });
            }
        }
        catch (Exception)
        {
            // Fallback in case of database or connection issues
            CategorySalesSeries.Clear();
            CategorySalesSeries.Add(new PieSeries<double> { Values = [45], Name = "Main Course (Demo)" });
            CategorySalesSeries.Add(new PieSeries<double> { Values = [25], Name = "Beverages (Demo)" });
            CategorySalesSeries.Add(new PieSeries<double> { Values = [20], Name = "Appetizers (Demo)" });
            CategorySalesSeries.Add(new PieSeries<double> { Values = [10], Name = "Desserts (Demo)" });
        }
    }
}
