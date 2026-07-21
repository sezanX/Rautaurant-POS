using System;
using System.Collections.Generic;
using System.Linq;
using KHAONPOS.Data.Entities;

namespace KHAONPOS.Data;

public static class DbSeeder
{
    public static void SeedOrderItemsIfEmpty(AppDbContext context)
    {
        try
        {
            var firstUser = context.Users.FirstOrDefault();
            if (firstUser == null)
            {
                firstUser = new User { Username = "admin", PasswordHash = "admin", Role = "Admin" };
                context.Users.Add(firstUser);
                context.SaveChanges();
            }

            if (!context.OrderItems.Any())
            {
                // Get the first 3 orders in the database
                var orders = context.Orders.Take(3).ToList();
                
                // If there are fewer than 3 orders, let's create them!
                while (orders.Count < 3)
                {
                    var newOrder = new Order
                    {
                        UserId = firstUser.Id,
                        OrderDate = DateTime.Now,
                        TotalAmount = 0m,
                        Status = "Completed"
                    };
                    context.Orders.Add(newOrder);
                    context.SaveChanges();
                    orders.Add(newOrder);
                }

                // Now, seed order items using the actual IDs of the orders we retrieved or created
                var orderItems = new List<OrderItem>
                {
                    // Order 1 (Burgers, Pizza, Drinks, Desserts)
                    new OrderItem { OrderId = orders[0].Id, MenuItemId = 1, Quantity = 2, UnitPrice = 9.99m }, // Classic Cheeseburger
                    new OrderItem { OrderId = orders[0].Id, MenuItemId = 3, Quantity = 1, UnitPrice = 15.99m }, // Pepperoni Pizza
                    new OrderItem { OrderId = orders[0].Id, MenuItemId = 5, Quantity = 2, UnitPrice = 2.50m }, // Coca Cola
                    new OrderItem { OrderId = orders[0].Id, MenuItemId = 7, Quantity = 1, UnitPrice = 5.99m }, // Chocolate Sundae

                    // Order 2 (Pizza, Drinks)
                    new OrderItem { OrderId = orders[1].Id, MenuItemId = 4, Quantity = 2, UnitPrice = 14.99m }, // Margherita Pizza
                    new OrderItem { OrderId = orders[1].Id, MenuItemId = 6, Quantity = 1, UnitPrice = 3.00m }, // Iced Tea

                    // Order 3 (Pizza)
                    new OrderItem { OrderId = orders[2].Id, MenuItemId = 3, Quantity = 1, UnitPrice = 15.99m } // Pepperoni Pizza
                };

                context.OrderItems.AddRange(orderItems);
                context.SaveChanges();

                // Align Order totals to match actual item sums
                orders[0].TotalAmount = 46.96m;
                orders[1].TotalAmount = 32.98m;
                orders[2].TotalAmount = 15.99m;
                context.SaveChanges();
            }

            // Seed historical weekly/monthly orders & payments if none exist in the past
            var pastOrdersCount = context.Orders.Count(o => o.OrderDate.Date < DateTime.Today);
            if (pastOrdersCount == 0)
            {
                // Seed historical weekly/monthly orders & payments
                var random = new Random();
                
                // 1. Seed past 6 days of daily orders/payments
                for (int i = 1; i <= 6; i++)
                {
                    var pastDate = DateTime.Today.AddDays(-i);
                    var order = new Order
                    {
                        UserId = firstUser.Id,
                        OrderDate = pastDate.AddHours(12),
                        TotalAmount = 0m,
                        Status = "Completed",
                        EstimatedCompletionTime = pastDate.AddHours(12).AddMinutes(15)
                    };
                    context.Orders.Add(order);
                    context.SaveChanges();

                    var item1 = new OrderItem { OrderId = order.Id, MenuItemId = 1, Quantity = random.Next(1, 3), UnitPrice = 9.99m };
                    var item2 = new OrderItem { OrderId = order.Id, MenuItemId = 3, Quantity = random.Next(1, 2), UnitPrice = 15.99m };
                    context.OrderItems.AddRange(item1, item2);
                    context.SaveChanges();

                    order.TotalAmount = item1.TotalPrice + item2.TotalPrice;
                    context.SaveChanges();

                    var payment = new Payment
                    {
                        OrderId = order.Id,
                        AmountPaid = order.TotalAmount,
                        PaymentDate = pastDate.AddHours(12).AddMinutes(30),
                        PaymentMethod = random.Next(2) == 0 ? "Card" : "Cash"
                    };
                    context.Payments.Add(payment);
                    context.SaveChanges();
                }

                // 2. Seed past 5 months of monthly orders/payments
                for (int i = 1; i <= 5; i++)
                {
                    var pastMonthDate = DateTime.Today.AddMonths(-i);
                    var order = new Order
                    {
                        UserId = firstUser.Id,
                        OrderDate = pastMonthDate,
                        TotalAmount = 0m,
                        Status = "Completed",
                        EstimatedCompletionTime = pastMonthDate.AddMinutes(15)
                    };
                    context.Orders.Add(order);
                    context.SaveChanges();

                    var item = new OrderItem { OrderId = order.Id, MenuItemId = 2, Quantity = random.Next(2, 5), UnitPrice = 12.99m };
                    context.OrderItems.Add(item);
                    context.SaveChanges();

                    order.TotalAmount = item.TotalPrice;
                    context.SaveChanges();

                    var payment = new Payment
                    {
                        OrderId = order.Id,
                        AmountPaid = order.TotalAmount,
                        PaymentDate = pastMonthDate.AddMinutes(30),
                        PaymentMethod = "Card"
                    };
                    context.Payments.Add(payment);
                    context.SaveChanges();
                }
            }

            // 3. For any completed order in the database, ensure it has a payment record
            var completedOrders = context.Orders.Where(o => o.Status == "Completed" && o.TotalAmount > 0).ToList();
            var existingPaymentOrderIds = context.Payments.Select(p => p.OrderId).ToHashSet();
            
            var rand = new Random();
            bool paymentAdded = false;
            foreach (var order in completedOrders)
            {
                if (!existingPaymentOrderIds.Contains(order.Id))
                {
                    var payment = new Payment
                    {
                        OrderId = order.Id,
                        AmountPaid = order.TotalAmount,
                        PaymentDate = order.OrderDate.AddMinutes(15),
                        PaymentMethod = rand.Next(2) == 0 ? "Card" : "Cash"
                    };
                    context.Payments.Add(payment);
                    paymentAdded = true;
                }
            }
            if (paymentAdded)
            {
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "None";
            throw new Exception($"Failed seeding data safely. Inner Exception: {innerMsg}", ex);
        }
    }

    public static void MigrateLocalImagesToDatabase(AppDbContext context)
    {
        try
        {
            var itemsToMigrate = context.MenuItems
                .Where(m => m.ImageData == null && m.ImagePath != null && m.ImagePath.Trim() != "")
                .ToList();

            bool changed = false;
            foreach (var item in itemsToMigrate)
            {
                // Ensure it's a local path, not a URL
                if (item.ImagePath!.Contains(":\\") || item.ImagePath.StartsWith("/") || item.ImagePath.StartsWith("."))
                {
                    if (System.IO.File.Exists(item.ImagePath))
                    {
                        item.ImageData = System.IO.File.ReadAllBytes(item.ImagePath);
                        context.MenuItems.Update(item);
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to migrate images: {ex.Message}");
        }
    }
}
