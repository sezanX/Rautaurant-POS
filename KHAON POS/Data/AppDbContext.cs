using System;
using Microsoft.EntityFrameworkCore;
using KHAONPOS.Data.Entities;

namespace KHAONPOS.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=ep-lively-poetry-azpra9fn.c-3.ap-southeast-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_QJ9rtifFqpD0;Ssl Mode=Require;Trust Server Certificate=true;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.MenuItem)
            .WithMany()
            .HasForeignKey(oi => oi.MenuItemId);

        // Data Seeder
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", PasswordHash = "admin", Role = "Admin" },
            new User { Id = 2, Username = "cashier1", PasswordHash = "1234", Role = "Cashier" },
            new User { Id = 3, Username = "kitchen1", PasswordHash = "kitchen", Role = "Kitchen" }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Burgers", IconName = "Hamburger" },
            new Category { Id = 2, Name = "Pizza", IconName = "Pizza" },
            new Category { Id = 3, Name = "Drinks", IconName = "CupWater" },
            new Category { Id = 4, Name = "Desserts", IconName = "IceCream" }
        );

        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem { Id = 1, Name = "Classic Cheeseburger", Description = "Beef patty with cheese", Price = 9.99m, CategoryId = 1, StockQuantity = 50, Barcode = "10001", PreparationTimeMinutes = 5, ImagePath = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400&q=80" },
            new MenuItem { Id = 2, Name = "Double Bacon Burger", Description = "Two beef patties with bacon", Price = 12.99m, CategoryId = 1, StockQuantity = 30, Barcode = "10002", PreparationTimeMinutes = 8, ImagePath = "https://images.unsplash.com/photo-1594212202875-8eb5a8820c75?w=400&q=80" },
            new MenuItem { Id = 3, Name = "Pepperoni Pizza", Description = "Large pepperoni pizza", Price = 15.99m, CategoryId = 2, StockQuantity = 20, Barcode = "20001", PreparationTimeMinutes = 12, ImagePath = "https://images.unsplash.com/photo-1628840042765-356cda07504e?w=400&q=80" },
            new MenuItem { Id = 4, Name = "Margherita Pizza", Description = "Classic cheese and tomato", Price = 14.99m, CategoryId = 2, StockQuantity = 25, Barcode = "20002", PreparationTimeMinutes = 10, ImagePath = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=400&q=80" },
            new MenuItem { Id = 5, Name = "Coca Cola", Description = "Can of Coke", Price = 2.50m, CategoryId = 3, StockQuantity = 200, Barcode = "30001", PreparationTimeMinutes = 1, ImagePath = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=400&q=80" },
            new MenuItem { Id = 6, Name = "Iced Tea", Description = "Fresh brewed iced tea", Price = 3.00m, CategoryId = 3, StockQuantity = 150, Barcode = "30002", PreparationTimeMinutes = 2, ImagePath = "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400&q=80" },
            new MenuItem { Id = 7, Name = "Chocolate Sundae", Description = "Vanilla ice cream with chocolate syrup", Price = 5.99m, CategoryId = 4, StockQuantity = 40, Barcode = "40001", PreparationTimeMinutes = 3, ImagePath = "https://images.unsplash.com/photo-1563805042-7684c8a9e9ce?w=400&q=80" }
        );

        modelBuilder.Entity<Order>().HasData(
            new Order { Id = 1, UserId = 2, OrderDate = DateTime.Today.AddHours(12), TotalAmount = 45.20m, Status = "Completed", EstimatedCompletionTime = DateTime.Today.AddHours(12).AddMinutes(15) },
            new Order { Id = 2, UserId = 2, OrderDate = DateTime.Today.AddHours(13), TotalAmount = 35.40m, Status = "Completed", EstimatedCompletionTime = DateTime.Today.AddHours(13).AddMinutes(10) },
            new Order { Id = 3, UserId = 2, OrderDate = DateTime.Today.AddHours(14), TotalAmount = 15.99m, Status = "Completed", EstimatedCompletionTime = DateTime.Today.AddHours(14).AddMinutes(12) }
        );

        modelBuilder.Entity<Payment>().HasData(
            new Payment { Id = 1, OrderId = 1, AmountPaid = 45.20m, PaymentDate = DateTime.Today.AddHours(12).AddMinutes(30), PaymentMethod = "Card" },
            new Payment { Id = 2, OrderId = 2, AmountPaid = 35.40m, PaymentDate = DateTime.Today.AddHours(13).AddMinutes(20), PaymentMethod = "Cash" },
            new Payment { Id = 3, OrderId = 3, AmountPaid = 15.99m, PaymentDate = DateTime.Today.AddHours(14).AddMinutes(15), PaymentMethod = "Card" }
        );
    }
}
