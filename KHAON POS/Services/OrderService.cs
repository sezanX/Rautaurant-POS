using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KHAONPOS.Data;
using KHAONPOS.Data.Entities;

namespace KHAONPOS.Services;

public class OrderService(AppDbContext context) : IOrderService
{
    private readonly AppDbContext _context = context;

    public async Task<Order> CreateOrderAsync(int userId)
    {
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            Status = "Draft",
            TotalAmount = 0
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public Task<List<Order>> GetActiveOrdersAsync()
    {
        return _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.Status == "Pending" || o.Status == "Preparing" || o.Status == "Served")
            .OrderBy(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order> AddItemToOrderAsync(int orderId, int menuItemId, int quantity, string? remarks = null)
    {
        var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new Exception("Order not found");

        var menuItem = await _context.MenuItems.FindAsync(menuItemId)
            ?? throw new Exception("Menu item not found");

        var existingItem = order.OrderItems.FirstOrDefault(oi => oi.MenuItemId == menuItemId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            if (!string.IsNullOrEmpty(remarks))
            {
                existingItem.Remarks = remarks;
            }
            order.TotalAmount += (quantity * (menuItem.Price + existingItem.ExtraCharge));
        }
        else
        {
            var orderItem = new OrderItem
            {
                OrderId = orderId,
                MenuItemId = menuItemId,
                MenuItem = menuItem,
                Quantity = quantity,
                UnitPrice = menuItem.Price,
                Remarks = remarks,
                ExtraCharge = 0
            };
            order.OrderItems.Add(orderItem);
            order.TotalAmount += (quantity * menuItem.Price);
        }

        await _context.SaveChangesAsync();
        return order;
    }

    public async Task RemoveItemFromOrderAsync(int orderItemId)
    {
        var orderItem = await _context.OrderItems.Include(oi => oi.Order).FirstOrDefaultAsync(oi => oi.Id == orderItemId);
        if (orderItem?.Order != null)
        {
            orderItem.Order.TotalAmount -= (orderItem.Quantity * (orderItem.UnitPrice + orderItem.ExtraCharge));
            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateOrderStatusAsync(int orderId, string status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Status = status;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Payment> ProcessPaymentAsync(int orderId, decimal amount, string paymentMethod)
    {
        var order = await _context.Orders.FindAsync(orderId)
            ?? throw new Exception("Order not found");

        var payment = new Payment
        {
            OrderId = orderId,
            AmountPaid = amount,
            PaymentMethod = paymentMethod,
            PaymentDate = DateTime.Now
        };

        order.Status = "Paid";

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task UpdateOrderItemQuantityAsync(int orderItemId, int quantity)
    {
        var orderItem = await _context.OrderItems.Include(oi => oi.Order).FirstOrDefaultAsync(oi => oi.Id == orderItemId);
        if (orderItem?.Order != null)
        {
            var difference = quantity - orderItem.Quantity;
            orderItem.Quantity = quantity;
            orderItem.Order.TotalAmount += (difference * (orderItem.UnitPrice + orderItem.ExtraCharge));

            if (orderItem.Quantity <= 0)
            {
                _context.OrderItems.Remove(orderItem);
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateOrderItemRemarksAsync(int orderItemId, string remarks)
    {
        var orderItem = await _context.OrderItems.FindAsync(orderItemId);
        if (orderItem != null)
        {
            orderItem.Remarks = remarks;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateOrderItemExtraChargeAsync(int orderItemId, decimal extraCharge)
    {
        var orderItem = await _context.OrderItems.Include(oi => oi.Order).FirstOrDefaultAsync(oi => oi.Id == orderItemId);
        if (orderItem?.Order != null)
        {
            var diff = extraCharge - orderItem.ExtraCharge;
            orderItem.ExtraCharge = extraCharge;
            orderItem.Order.TotalAmount += (orderItem.Quantity * diff);
            await _context.SaveChangesAsync();
        }
    }

    public async Task PlaceOrderAsync(int orderId, int estimatedTimeMinutes)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Status = "Pending";
            order.EstimatedCompletionTime = DateTime.Now.AddMinutes(estimatedTimeMinutes);

            // Automatically process payment since this is a prepaid system
            var payment = new Payment
            {
                OrderId = orderId,
                AmountPaid = order.TotalAmount,
                PaymentMethod = "Cash",
                PaymentDate = DateTime.Now
            };
            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();
        }
    }

    public async Task AddTimeToOrderAsync(int orderId, int minutesToAdd)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order?.EstimatedCompletionTime.HasValue is true)
        {
            order.EstimatedCompletionTime = order.EstimatedCompletionTime.Value.AddMinutes(minutesToAdd);
            await _context.SaveChangesAsync();
        }
    }
}
