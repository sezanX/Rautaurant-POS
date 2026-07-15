using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.Data.Entities;

namespace RestaurantPOS.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(int? tableId, int userId);
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<List<Order>> GetActiveOrdersAsync();
    Task<Order> AddItemToOrderAsync(int orderId, int menuItemId, int quantity, string? remarks = null);
    Task RemoveItemFromOrderAsync(int orderItemId);
    Task UpdateOrderStatusAsync(int orderId, string status);
    Task<Payment> ProcessPaymentAsync(int orderId, decimal amount, string paymentMethod);
    Task UpdateOrderItemQuantityAsync(int orderItemId, int quantity);
    Task UpdateOrderItemRemarksAsync(int orderItemId, string remarks);
    Task UpdateOrderItemExtraChargeAsync(int orderItemId, decimal extraCharge);
    Task PlaceOrderAsync(int orderId, int estimatedTimeMinutes);
    Task AddTimeToOrderAsync(int orderId, int minutesToAdd);
}
