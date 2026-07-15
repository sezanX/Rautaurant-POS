# Complete Application Tutorial & Explanation

This document serves as an in-depth guide to understanding the architecture of the KHAON POS application and provides a tutorial on how to navigate, maintain, and extend the codebase.

## 1. Architectural Overview

The application follows a strict **Model-View-ViewModel (MVVM)** pattern combined with **Dependency Injection (DI)**.

### Directory Structure
- **`/Data`**: Contains the `AppDbContext` (the EF Core gateway using Npgsql for PostgreSQL) and the `Entities` (the C# classes representing the database tables).
- **`/Services`**: Contains interfaces and business logic implementations. This decouples the database operations and API calls from the UI layer.
- **`/ViewModels`**: Contains the state and logic for the UI. ViewModels have zero knowledge of the WPF UI controls (no `using System.Windows.Controls;`). They expose `ICommand` properties for button clicks and `ObservableCollection` properties for lists.
- **`/Views`**: Contains the `.xaml` files. The UI binds directly to the properties in the ViewModels.
- **`/Converters`**: Contains classes that convert ViewModel data into UI-friendly data (e.g., converting a "Pending" status string into an Orange `SolidColorBrush`).

### Application Startup Flow
1. **`App.xaml.cs`**: The `OnStartup` method executes.
2. An `IHost` (Dependency Injection container) is built. All Services, ViewModels, and the `AppDbContext` are registered as singletons or transients.
3. EF Core's `dbContext.Database.EnsureCreated()` is called, checking if your connected PostgreSQL database (on Neon.tech) has the `RestaurantPosDB` schema. If not, it automatically creates the tables in the cloud and inserts the initial mock data (configured in `AppDbContext.cs`).
4. `MainWindow` is resolved from the DI container and displayed.

---

## 2. Deep Dive: How the MVVM Pattern is implemented

### The BaseViewModel & INotifyPropertyChanged
WPF needs to know when a property changes to update the screen. This is done via `INotifyPropertyChanged`. 
In this app, `BaseViewModel.cs` implements this. All ViewModels inherit from it.
When setting a property, you use `SetProperty(ref _myField, value)` which automatically alerts the UI.

### Commands
Instead of `Click="Button_Click"` events in the code-behind (`.xaml.cs`), MVVM uses `ICommand`. 
`RelayCommand.cs` provides this functionality.
For example, in `LoginViewModel.cs`:
```csharp
public ICommand LoginCommand { get; }
public LoginViewModel() {
    LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
}
```
The UI binds to this: `<Button Command="{Binding LoginCommand}" />`.

---

## 3. Tutorial: Extending the Application

Let's walk through the exact steps required to add a new "Employee Management" feature for Administrators.

### Step 1: Add a new ViewModel
1. Create `EmployeeViewModel.cs` in the `/ViewModels` folder.
2. Inherit from `BaseViewModel`.
3. Add an `ObservableCollection<User>` to hold the users.
4. Request `AppDbContext` via the constructor to fetch the users.

```csharp
public class EmployeeViewModel : BaseViewModel
{
    private readonly AppDbContext _context;
    public ObservableCollection<User> Employees { get; } = new();

    public EmployeeViewModel(AppDbContext context) {
        _context = context;
        LoadEmployees();
    }
    
    private void LoadEmployees() {
        foreach (var user in _context.Users.ToList()) {
            Employees.Add(user);
        }
    }
}
```

### Step 2: Register the ViewModel in Dependency Injection
Open `App.xaml.cs`. Inside the `.ConfigureServices` block, add:
```csharp
services.AddTransient<EmployeeViewModel>();
```

### Step 3: Create the UI View
1. Create `EmployeeView.xaml` (UserControl) in the `/Views` folder.
2. Use a MaterialDesign DataGrid or ItemsControl to display the `Employees` collection.

```xml
<UserControl x:Class="RestaurantPOS.Views.EmployeeView" ...>
    <DataGrid ItemsSource="{Binding Employees}" AutoGenerateColumns="True" />
</UserControl>
```

### Step 4: Hook it up to the Navigation System
1. Open `MainWindow.xaml` or `DashboardView.xaml` (depending on where you want the feature to live).
2. Map the new ViewModel to the new View in the `<Window.Resources>` or `<UserControl.Resources>` section:
```xml
<DataTemplate DataType="{x:Type viewmodels:EmployeeViewModel}">
    <views:EmployeeView />
</DataTemplate>
```
3. Add a new Navigation Button (using a `RadioButton` or `Button`).
4. Update the parent ViewModel's switch statement to route the user correctly.

You have now successfully extended the application using strict MVVM and DI!

---

## 4. Troubleshooting & Advanced Topics

### Database Migrations
Currently, the app uses `EnsureCreated()` which is great for quick prototyping and auto-provisioning the cloud database. If you want to evolve the database schema (e.g., adding a new column to an existing table), you have two options:
1. **Drop & Recreate:** Delete the tables manually in your Neon.tech SQL dashboard, and run the app. `EnsureCreated()` will rebuild them with the new columns. (Warning: erases data).
2. **EF Core Migrations:** Delete `EnsureCreated()` from `App.xaml.cs`. Run `dotnet ef migrations add InitialCreate` in your terminal, followed by `dotnet ef database update`.

### Cloud Database Issues
If you experience a crash on startup, it is almost certainly a connection issue with PostgreSQL.
Verify that:
1. You have an active internet connection.
2. Your connection string in `AppDbContext.cs` is perfectly accurate and contains the password.
3. Your cloud provider (Neon.tech) is not currently experiencing an outage.
