using System.Threading.Tasks;
using RestaurantPOS.Data.Entities;

namespace RestaurantPOS.Services;

public interface IReportingService
{
    Task<string> GenerateReceiptPdfAsync(Order order, Payment payment);
    Task<decimal> GetTotalSalesAsync(System.DateTime date);
    Task<int> GetOrderCountAsync(System.DateTime date);
    Task<System.Collections.Generic.List<Order>> GetRecentOrdersAsync(int count = 50);
    Task<System.Collections.Generic.List<RestaurantPOS.Data.Models.TopItemDTO>> GetTopItemsAsync(int count = 5);
}
