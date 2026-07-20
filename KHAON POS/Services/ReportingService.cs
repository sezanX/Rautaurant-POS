using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using KHAONPOS.Data;
using KHAONPOS.Data.Entities;
using KHAONPOS.Data.Models;

namespace KHAONPOS.Services;

public class ReportingService : IReportingService
{
    private readonly AppDbContext _context;

    public ReportingService(AppDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<string> GenerateReceiptPdfAsync(Order order, Payment? payment = null)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"Receipt_{order.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.ContinuousSize(80, Unit.Millimetre);
                page.Margin(4, Unit.Millimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9.5f).FontFamily(Fonts.Arial));

                page.Content().MinHeight(100, Unit.Millimetre).Column(column =>
                {
                    column.Spacing(4);

                    // Header
                    column.Item().AlignCenter().Text("KHAON POS RECEIPT").FontSize(14).Bold();
                    column.Item().AlignCenter().Text($"Order #{order.Id}").FontSize(11).SemiBold();
                    column.Item().AlignCenter().Text($"Date: {order.OrderDate:dd/MM/yyyy h:mm:ss tt}").FontSize(9);

                    column.Item().PaddingVertical(2).LineHorizontal(0.75f).LineColor(Colors.Grey.Medium);

                    // Items list
                    foreach (var item in order.OrderItems)
                    {
                        column.Item().Row(row =>
                        {
                            var itemName = item.MenuItem?.Name ?? "Item";
                            row.RelativeItem().Text($"{item.Quantity}x {itemName}").Bold();
                            row.AutoItem().Text($"US${item.TotalPrice:F2}").Bold();
                        });

                        if (!string.IsNullOrEmpty(item.Remarks))
                        {
                            column.Item().PaddingLeft(6).Text($"Note: {item.Remarks}").FontSize(8.5f).Italic().FontColor(Colors.Grey.Darken1);
                        }

                        if (item.ExtraCharge > 0)
                        {
                            column.Item().PaddingLeft(6).Text($"+ Extra: US${item.ExtraCharge * item.Quantity:F2}").FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                        }
                    }

                    column.Item().PaddingVertical(2).LineHorizontal(0.75f).LineColor(Colors.Grey.Medium);

                    // Total
                    column.Item().AlignRight().Text($"Total: US${order.TotalAmount:F2}").FontSize(12).Bold();

                    if (payment != null)
                    {
                        column.Item().AlignRight().Text($"Paid ({payment.PaymentMethod}): US${payment.AmountPaid:F2}").FontSize(9);
                    }

                    column.Item().PaddingTop(6).AlignCenter().Text("Thank you!").Italic().FontSize(9);
                });
            });
        })
        .GeneratePdf(filePath);

        return Task.FromResult(filePath);
    }

    public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var paymentsQuery = _context.Payments.AsQueryable();
        if (startDate.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.PaymentDate >= startDate.Value);
        if (endDate.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.PaymentDate <= endDate.Value);

        var totalFromPayments = await paymentsQuery.SumAsync(p => (decimal?)p.AmountPaid) ?? 0m;

        if (totalFromPayments > 0m)
            return totalFromPayments;

        var ordersQuery = _context.Orders.Where(o => o.Status == "Completed" || o.Status == "Paid");
        if (startDate.HasValue)
            ordersQuery = ordersQuery.Where(o => o.OrderDate >= startDate.Value);
        if (endDate.HasValue)
            ordersQuery = ordersQuery.Where(o => o.OrderDate <= endDate.Value);

        return await ordersQuery.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
    }

    public async Task<int> GetOrderCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var ordersQuery = _context.Orders.Where(o => o.Status != "Cancelled" && (o.TotalAmount > 0 || o.OrderItems.Any()));
        if (startDate.HasValue)
            ordersQuery = ordersQuery.Where(o => o.OrderDate >= startDate.Value);
        if (endDate.HasValue)
            ordersQuery = ordersQuery.Where(o => o.OrderDate <= endDate.Value);

        return await ordersQuery.CountAsync();
    }

    public async Task<int> GetActiveCustomersCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var ordersQuery = _context.Orders.Where(o => o.Status != "Cancelled");
        if (startDate.HasValue)
            ordersQuery = ordersQuery.Where(o => o.OrderDate >= startDate.Value);
        if (endDate.HasValue)
            ordersQuery = ordersQuery.Where(o => o.OrderDate <= endDate.Value);

        return await ordersQuery.CountAsync();
    }

    public Task<List<Order>> GetRecentOrdersAsync(int count = 50)
    {
        return _context.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<TopItemDTO>> GetTopItemsAsync(int count = 5)
    {
        var groupedData = await _context.OrderItems
            .GroupBy(oi => oi.MenuItemId)
            .Select(g => new
            {
                MenuItemId = g.Key,
                UnitsSold = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(x => x.UnitsSold)
            .Take(count)
            .ToListAsync();

        var topItems = new List<TopItemDTO>();

        if (groupedData.Count == 0)
        {
            var fallbackItems = await _context.MenuItems
                .Include(m => m.Category)
                .OrderBy(m => m.Name)
                .Take(count)
                .ToListAsync();

            foreach (var menuItem in fallbackItems)
            {
                topItems.Add(new TopItemDTO
                {
                    Name = menuItem.Name,
                    IconName = menuItem.Category?.IconName ?? "Food",
                    UnitsSold = 0
                });
            }

            return topItems;
        }

        var itemIds = groupedData.Select(d => d.MenuItemId);
        var menuItemsDict = await _context.MenuItems
            .Include(m => m.Category)
            .Where(m => itemIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);

        foreach (var data in groupedData)
        {
            menuItemsDict.TryGetValue(data.MenuItemId, out var menuItem);
            topItems.Add(new TopItemDTO
            {
                Name = menuItem?.Name ?? "Unknown",
                IconName = menuItem?.Category?.IconName ?? "Food",
                UnitsSold = data.UnitsSold
            });
        }

        return topItems;
    }

    public async Task<Dictionary<string, double>> GetCategorySalesAsync()
    {
        var items = await _context.OrderItems
            .Include(oi => oi.MenuItem)
            .ThenInclude(m => m!.Category)
            .ToListAsync();

        var categorySales = items
            .GroupBy(oi => oi.MenuItem?.Category?.Name ?? "Other")
            .ToDictionary(
                g => g.Key,
                g => g.Sum(oi => (double)oi.Quantity * (double)(oi.UnitPrice + oi.ExtraCharge))
            );

        return categorySales;
    }

    public async Task<(string[] Labels, decimal[] Values)> GetWeeklySalesAsync()
    {
        var startDate = DateTime.Today.AddDays(-6);

        var dailySales = await _context.Payments
            .Where(p => p.PaymentDate.Date >= startDate.Date)
            .GroupBy(p => p.PaymentDate.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(p => p.AmountPaid) })
            .ToListAsync();

        var labels = new string[7];
        var values = new decimal[7];

        for (int i = 0; i < 7; i++)
        {
            var date = startDate.AddDays(i);
            labels[i] = date.ToString("ddd");
            values[i] = dailySales.FirstOrDefault(x => x.Date == date.Date)?.Total ?? 0m;
        }

        return (labels, values);
    }

    public async Task<(string[] Labels, decimal[] Values)> GetMonthlySalesAsync()
    {
        var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);

        var monthlySales = await _context.Payments
            .Where(p => p.PaymentDate.Date >= startDate.Date)
            .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(p => p.AmountPaid) })
            .ToListAsync();

        var labels = new string[6];
        var values = new decimal[6];

        for (int i = 0; i < 6; i++)
        {
            var monthDate = startDate.AddMonths(i);
            labels[i] = monthDate.ToString("MMM");
            values[i] = monthlySales.FirstOrDefault(x => x.Year == monthDate.Year && x.Month == monthDate.Month)?.Total ?? 0m;
        }

        return (labels, values);
    }
}
