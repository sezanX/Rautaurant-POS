using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RestaurantPOS.Data;
using RestaurantPOS.Data.Entities;

namespace RestaurantPOS.Services;

public class ReportingService : IReportingService
{
    private readonly AppDbContext _context;

    public ReportingService(AppDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GenerateReceiptPdfAsync(Order order, Payment payment)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"Receipt_{order.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().Element(ComposeHeader);
                page.Content().Element(x => ComposeContent(x, order, payment));
                page.Footer().Element(ComposeFooter);
            });
        })
        .GeneratePdf(filePath);

        return await Task.FromResult(filePath);
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("KHAON POS").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text("123 Main Street, Cityville");
                column.Item().Text("Phone: (555) 123-4567");
            });
        });
    }

    private void ComposeContent(IContainer container, Order order, Payment payment)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(5);

            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Order #: {order.Id}");
                row.RelativeItem().AlignRight().Text($"Date: {order.OrderDate:g}");
            });

            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Table: {(order.Table != null ? order.Table.TableNumber.ToString() : "Takeout")}");
                row.RelativeItem().AlignRight().Text($"Cashier: {(order.User != null ? order.User.Username : "N/A")}");
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn();
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(50);
                });

                table.Header(header =>
                {
                    header.Cell().Text("Qty").SemiBold();
                    header.Cell().Text("Item").SemiBold();
                    header.Cell().AlignRight().Text("Unit").SemiBold();
                    header.Cell().AlignRight().Text("Total").SemiBold();
                    
                    header.Cell().ColumnSpan(4).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
                });

                foreach (var item in order.OrderItems)
                {
                    table.Cell().PaddingVertical(2).Text(item.Quantity.ToString());
                    table.Cell().PaddingVertical(2).Text(item.MenuItem?.Name ?? "Unknown");
                    table.Cell().PaddingVertical(2).AlignRight().Text($"${item.UnitPrice:F2}");
                    table.Cell().PaddingVertical(2).AlignRight().Text($"${(item.Quantity * item.UnitPrice):F2}");
                }
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().AlignRight().Text($"Total: ${order.TotalAmount:F2}").FontSize(14).SemiBold();
            column.Item().AlignRight().Text($"Paid ({payment.PaymentMethod}): ${payment.AmountPaid:F2}");
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text("Thank you for your business!").Italic().FontSize(10);
    }

    public async Task<decimal> GetTotalSalesAsync(DateTime date)
    {
        return await _context.Payments
            .Where(p => p.PaymentDate.Date == date.Date)
            .SumAsync(p => p.AmountPaid);
    }

    public async Task<int> GetOrderCountAsync(DateTime date)
    {
        return await _context.Orders
            .Where(o => o.OrderDate.Date == date.Date)
            .CountAsync();
    }

    public async Task<System.Collections.Generic.List<Order>> GetRecentOrdersAsync(int count = 50)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Table)
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<System.Collections.Generic.List<RestaurantPOS.Data.Models.TopItemDTO>> GetTopItemsAsync(int count = 5)
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

        var topItems = new System.Collections.Generic.List<RestaurantPOS.Data.Models.TopItemDTO>();

        if (groupedData.Count == 0)
        {
            var fallbackItems = await _context.MenuItems
                .Include(m => m.Category)
                .OrderBy(m => m.Name)
                .Take(count)
                .ToListAsync();

            foreach (var menuItem in fallbackItems)
            {
                topItems.Add(new RestaurantPOS.Data.Models.TopItemDTO
                {
                    Name = menuItem.Name,
                    IconName = menuItem.Category?.IconName ?? "Food",
                    UnitsSold = 0
                });
            }

            return topItems;
        }

        foreach(var data in groupedData) 
        {
             var menuItem = await _context.MenuItems
                 .Include(m => m.Category)
                 .FirstOrDefaultAsync(m => m.Id == data.MenuItemId);
                 
             topItems.Add(new RestaurantPOS.Data.Models.TopItemDTO
             {
                 Name = menuItem?.Name ?? "Unknown",
                 IconName = menuItem?.Category?.IconName ?? "Food",
                 UnitsSold = data.UnitsSold
             });
        }
        
        return topItems;
    }
}
