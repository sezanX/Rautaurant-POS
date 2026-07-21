# KHAON POS — Developer Architecture & Extension Guide

Welcome to the comprehensive technical documentation and developer tutorial for **KHAON POS**. This document is designed for software engineers and maintainers who want to understand the application's architecture, data flows, core components, and how to extend or customize the codebase.

---

## 📐 1. Architectural Blueprint & Design Patterns

KHAON POS is built on modern Microsoft .NET 8 desktop architectural standards, combining a strict **Model-View-ViewModel (MVVM)** pattern with **Dependency Injection (DI)** and **Entity Framework Core 8**.

```
                           ┌──────────────────────────┐
                           │   App.xaml.cs (IHost)    │
                           │  Dependency Injection    │
                           └─────────────┬────────────┘
                                         │ Bootstraps
                                         ▼
┌──────────────────────┐        ┌──────────────────────┐        ┌──────────────────────┐
│     WPF Views        │ Binds  │      ViewModels      │ Calls  │    Service Layer     │
│   (UserControls)     │◄──────►│ (BaseViewModel / DI) │───────►│  (IOrderService, etc)│
└──────────────────────┘        └──────────────────────┘        └──────────┬───────────┘
                                                                           │ Queries
                                                                           ▼
                                                                ┌──────────────────────┐
                                                                │  EF Core DbContext   │
                                                                │(PostgreSQL / Npgsql) │
                                                                └──────────────────────┘
```

### Key Architectural Principles:
1. **Separation of Concerns**:
   - **Views (`/Views`)**: Pure XAML presentation layer with zero business logic.
   - **ViewModels (`/ViewModels`)**: Contain UI state, command logic, and data properties. ViewModels have no direct references to WPF UI controls (`System.Windows.Controls`).
   - **Services (`/Services`)**: Encapsulate core business logic, PDF generation, calculation algorithms, and database queries.
   - **Data Layer (`/Data`)**: Handles database entity definitions, EF Core mapping, auto-migration, and seed data.
2. **Dependency Injection**:
   - Configured in `App.xaml.cs` via `Microsoft.Extensions.Hosting`.
   - All ViewModels, Services, and `AppDbContext` instances are registered in the DI container.
3. **Data Binding & Change Notification**:
   - ViewModels inherit from `BaseViewModel`, which implements `INotifyPropertyChanged`.
   - Commands use `RelayCommand` to bind user interactions (button clicks) directly to C# ViewModel methods.

---

## 📂 2. Directory & Component Breakdown

```
KHAON POS/
├── Data/
│   ├── AppDbContext.cs            # EF Core DbContext with Npgsql PostgreSQL configuration
│   ├── DbSeeder.cs                # Automatic initial database populator on application startup
│   └── Entities/
│       ├── User.cs                # Staff user account entity (Admin, Cashier, Kitchen)
│       ├── Category.cs            # Menu item category entity
│       ├── MenuItem.cs            # Menu item entity (price, stock, barcode, prep time)
│       ├── Order.cs               # Order header entity (status, total, prep time estimate)
│       ├── OrderItem.cs           # Order line item entity (quantity, unit price, remarks, extra charge)
│       └── Payment.cs             # Payment record entity (amount, method, date)
├── Services/
│   ├── IOrderService.cs           # Interface for order creation, cart updates, and KDS queue
│   ├── OrderService.cs            # EF Core implementation for orders
│   ├── IInventoryService.cs       # Interface for menu item, category, and stock CRUD
│   ├── InventoryService.cs        # EF Core implementation for inventory
│   ├── IReportingService.cs       # Interface for analytics, metrics, and QuestPDF thermal receipts
│   └── ReportingService.cs        # Reporting & QuestPDF implementation
├── ViewModels/
│   ├── BaseViewModel.cs           # Abstract base class implementing INotifyPropertyChanged
│   ├── RelayCommand.cs            # ICommand implementation for MVVM command binding
│   ├── MainViewModel.cs           # Root ViewModel managing top-level navigation
│   ├── LoginViewModel.cs          # User authentication ViewModel
│   ├── CashierViewModel.cs        # Cashier POS station ViewModel (cart, prep estimation, checkout)
│   ├── KitchenViewModel.cs        # KDS Live queue ViewModel (10s polling, 1s timer, sound alert)
│   ├── AdminDashboardViewModel.cs # Admin tab routing ViewModel
│   ├── AdminOverviewViewModel.cs  # Executive KPI dashboard & chart ViewModel
│   ├── AdminAnalyticsViewModel.cs # Sales analytics & trend period breakdown
│   ├── AdminInventoryViewModel.cs # Menu item CRUD ViewModel
│   ├── AdminCategoryViewModel.cs  # Category CRUD ViewModel
│   ├── AdminUsersViewModel.cs     # Staff user management ViewModel
│   └── AdminReportsViewModel.cs   # Date-filtered sales report ViewModel
├── Views/
│   ├── MainWindow.xaml            # Main window host with dynamic ContentControl binding
│   ├── LoginView.xaml             # Login screen view
│   ├── CashierView.xaml           # Cashier POS station view
│   ├── KitchenView.xaml           # Kitchen Display System view
│   └── AdminDashboardView.xaml    # Admin dashboard view with navigation sidebar
└── Converters/
    ├── StatusToColorConverter.cs   # Converts order status / time remaining to WPF SolidColorBrush
    ├── CurrencyConverter.cs        # Formats decimal values as currency strings ($0.00)
    ├── RoleSelectedConverter.cs    # Converts user roles to UI state
    └── ActiveTabConverter.cs       # Manages active tab highlight states
```

---

## 🔍 3. Deep Dive into Core Subsystems

### A. Application Startup & Dependency Injection (`App.xaml.cs`)
When the application launches:
1. `App()` constructor initializes the `IHost` builder.
2. Services, ViewModels, and `AppDbContext` are registered:
   ```csharp
   services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);
   services.AddTransient<IOrderService, OrderService>();
   services.AddTransient<IInventoryService, InventoryService>();
   services.AddTransient<IReportingService, ReportingService>();
   services.AddSingleton<MainViewModel>();
   ```
3. In `OnStartup()`:
   - `dbContext.Database.Migrate()` is invoked to automatically create/update database tables in PostgreSQL.
   - `DbSeeder.SeedOrderItemsIfEmpty(dbContext)` populates default users, categories, menu items, and historical orders.
   - `MainWindow` is resolved from DI and shown to the user.

### B. Authentication & View Routing
1. `LoginViewModel` captures the entered username, password, and selected role.
2. It queries `AppDbContext` for a matching user entity.
3. Upon successful login, `LoginViewModel` triggers the `LoginSuccessful` event with the authenticated `User` object.
4. `MainViewModel` handles the event and sets `CurrentViewViewModel` to the corresponding landing view:
   - `Role == "Admin"` ➔ `AdminDashboardViewModel`
   - `Role == "Cashier"` ➔ `CashierViewModel`
   - `Role == "Kitchen"` ➔ `KitchenViewModel`

### C. Cashier POS & Thermal Receipt Engine
1. `CashierViewModel` maintains a draft order in `AppDbContext`.
2. As the cashier adds items or custom remarks (*e.g., "Extra Spicy", extra charge +$1.50*), `OrderService.AddItemToOrderAsync` recalculates:
   $$\text{Total Amount} = \sum (\text{UnitPrice} + \text{ExtraCharge}) \times \text{Quantity}$$
   $$\text{Estimated Time (mins)} = \sum (\text{Item Prep Time} \times \text{Quantity})$$
3. Upon checkout, `OrderService.PlaceOrderAsync` updates the order status to `"Pending"`, calculates `EstimatedCompletionTime`, creates a `Payment` record, and calls `ReportingService.GenerateReceiptPdfAsync`.
4. `ReportingService` utilizes **QuestPDF** to format an 80mm thermal receipt PDF file saved in `Path.GetTempPath()`, launching the default system PDF reader for instant thermal printing.

### D. Kitchen Display System (KDS) Queue & Timers
1. `KitchenViewModel` runs two simultaneous timers upon loading:
   - **10-Second DB Polling Timer**: Asynchronously queries active orders where status is `"Pending"` or `"Preparing"`, updating the UI grid.
   - **1-Second UI Tick Timer**: Recalculates `TimeRemaining` for every displayed order ticket.
2. Dynamic Color Indicators:
   - **TimeRemaining > 60s**: Green brush (`#66BD76`).
   - **56s ≤ TimeRemaining ≤ 60s**: Orange brush (`#FF9800`) + **`Console.Beep()` alert sound**.
   - **TimeRemaining < 0s**: Overdue Red brush (`#E53935`).
3. Staff Controls:
   - **+5 Mins Button**: Calls `OrderService.AddTimeToOrderAsync` to extend prep time by 5 minutes.
   - **Mark as Prepared**: Updates order status to `"Served"` in the database and removes the ticket card.

---

## 🛠️ 4. Step-by-Step Tutorial: Extending the Application

Let's walk through adding a new feature: **"Discount Management System"** for Admins.

### Step 1: Define the Entity Model
Create `Discount.cs` in `KHAON POS/Data/Entities/`:
```csharp
namespace KHAONPOS.Data.Entities;

public class Discount
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### Step 2: Register in `AppDbContext`
Open `KHAON POS/Data/AppDbContext.cs` and add the `DbSet`:
```csharp
public DbSet<Discount> Discounts { get; set; }
```

### Step 3: Add Service Methods
Update `IInventoryService.cs` and `InventoryService.cs`:
```csharp
public interface IInventoryService
{
    // ... existing methods
    Task<List<Discount>> GetDiscountsAsync();
    Task AddDiscountAsync(Discount discount);
}
```

### Step 4: Create the ViewModel
Create `AdminDiscountViewModel.cs` in `KHAON POS/ViewModels/`:
```csharp
using System.Collections.ObjectModel;
using System.Windows.Input;
using KHAONPOS.Data.Entities;
using KHAONPOS.Services;

namespace KHAONPOS.ViewModels;

public class AdminDiscountViewModel : BaseViewModel
{
    private readonly IInventoryService _inventoryService;
    public ObservableCollection<Discount> Discounts { get; } = new();

    public AdminDiscountViewModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
        LoadDiscountsCommand = new RelayCommand(async () => await LoadDiscountsAsync());
        LoadDiscountsCommand.Execute(null);
    }

    public ICommand LoadDiscountsCommand { get; }

    private async Task LoadDiscountsAsync()
    {
        Discounts.Clear();
        var discounts = await _inventoryService.GetDiscountsAsync();
        foreach (var d in discounts) Discounts.Add(d);
    }
}
```

### Step 5: Register ViewModel in Dependency Injection
Open `KHAON POS/App.xaml.cs` and register the new ViewModel inside `ConfigureServices`:
```csharp
services.AddTransient<AdminDiscountViewModel>();
```

### Step 6: Create the WPF View
Create `AdminDiscountView.xaml` in `KHAON POS/Views/`:
```xml
<UserControl x:Class="RestaurantPOS.Views.AdminDiscountView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="20">
        <DataGrid ItemsSource="{Binding Discounts}" AutoGenerateColumns="True" />
    </Grid>
</UserControl>
```

### Step 7: Wire Up DataTemplate in Dashboard
Open `KHAON POS/Views/AdminDashboardView.xaml` and map the ViewModel to the View in `<UserControl.Resources>`:
```xml
<DataTemplate DataType="{x:Type viewmodels:AdminDiscountViewModel}">
    <views:AdminDiscountView />
</DataTemplate>
```

Add a navigation sidebar button bound to switch the active view model to `AdminDiscountViewModel`. You have now seamlessly added a new feature while maintaining clean MVVM separation!

---

## 🗄️ 5. Database Administration & Migrations

### Switch Database Provider / Connection String
In `KHAON POS/Data/AppDbContext.cs`:
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        // Cloud PostgreSQL (Neon.tech) or Local PostgreSQL connection string
        optionsBuilder.UseNpgsql("Host=localhost;Database=RestaurantPosDB;Username=postgres;Password=yourpassword;");
    }
}
```

### Adding EF Core Database Migrations
To create and apply a migration after modifying entity models:
1. Open PowerShell / Terminal in `KHAON POS/`:
   ```powershell
   cd "d:\Software Dev Projects\Rautaurant-POS-main\KHAON POS"
   ```
2. Add a new migration:
   ```powershell
   dotnet ef migrations add AddDiscountFeature
   ```
3. Apply migration to the PostgreSQL database:
   ```powershell
   dotnet ef database update
   ```

---

## ❓ 6. Troubleshooting & FAQ

### 1. Application crashes on startup with "Startup Error"
- **Cause**: Database connection failure.
- **Solution**: Verify active internet connection (for Neon.tech cloud DB) or check that your local PostgreSQL service is running. Check `crash.log` created in the application root folder for detailed stack trace logs.

### 2. Receipt PDF printing issue
- **Cause**: Missing QuestPDF Community License initializer or missing system PDF viewer.
- **Solution**: QuestPDF is configured for continuous 80mm layout. Ensure default PDF application (e.g., Microsoft Edge, Adobe Reader) is configured on Windows to handle `.pdf` files.

### 3. UI doesn't update when data changes
- **Cause**: Property notification omitted.
- **Solution**: Ensure ViewModel properties use `SetProperty(ref _field, value)` inherited from `BaseViewModel` instead of basic auto-properties.

