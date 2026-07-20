using System.Threading.Tasks;
using KHAONPOS.Data.Entities;

namespace KHAONPOS.Services;

public interface IReportingService
{
    Task<string> GenerateReceiptPdfAsync(Order order, Payment? payment = null);
    Task<decimal> GetTotalSalesAsync(System.DateTime? startDate = null, System.DateTime? endDate = null);
    Task<int> GetOrderCountAsync(System.DateTime? startDate = null, System.DateTime? endDate = null);
    Task<int> GetActiveCustomersCountAsync(System.DateTime? startDate = null, System.DateTime? endDate = null);
    Task<System.Collections.Generic.List<Order>> GetRecentOrdersAsync(int count = 50);
    Task<System.Collections.Generic.List<KHAONPOS.Data.Models.TopItemDTO>> GetTopItemsAsync(int count = 5);
    Task<System.Collections.Generic.Dictionary<string, double>> GetCategorySalesAsync();
    Task<(string[] Labels, decimal[] Values)> GetWeeklySalesAsync();
    Task<(string[] Labels, decimal[] Values)> GetMonthlySalesAsync();
}
