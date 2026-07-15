# Restaurant POS

A production-ready Desktop Restaurant Point-of-Sale (POS) application built on modern Microsoft technologies.

## Tech Stack
- **Framework**: .NET 8
- **UI Framework**: Windows Presentation Foundation (WPF)
- **UI Library**: Material Design in XAML
- **Architecture**: Strict MVVM (Model-View-ViewModel)
- **Database**: SQLite (Fully portable)
- **ORM**: Entity Framework Core 8
- **Reporting**: QuestPDF (for digital receipt generation)
- **Barcode & QR**: ZXing.Net
- **Charts & Analytics**: LiveCharts2 (LiveChartsCore.SkiaSharpView.WPF)
- **Dependency Injection**: Microsoft.Extensions.Hosting

## Features
- **Role-Based Authentication**: Separate views for Admin, Cashier, and Kitchen staff.
- **Dynamic POS Interface**: Category-filtered menu items, shopping cart, and automatic receipt generation.
- **Kitchen Kanban Board**: Live auto-refreshing board showing active orders and their preparation status.
- **Admin Dashboard**: Real-time sales KPI tracking and a mock visual trend chart.
- **Automated Database Seeding**: Automatic EF Core database creation and population on startup.

## Prerequisites
To compile or run the source code, you must have the following installed on your Windows machine:
1. [**Visual Studio 2022**](https://visualstudio.microsoft.com/) (Recommended) OR the [**.NET 8.0 SDK**](https://dotnet.microsoft.com/download/dotnet/8.0).

Note: If you were provided with a published `.exe` file, you do not need to install anything! The application is fully portable and uses an embedded SQLite database.

## Quick Start

### Running from the Terminal
1. Open your terminal/PowerShell.
2. Navigate to the root directory of this repository (where the `.csproj` file is located).
3. Run the application:
   ```powershell
   dotnet run
   ```


## Default Login Credentials
The database automatically seeds these users on startup:

| Role | Username | Password |
|---|---|---|
| Administrator | `admin` | `admin` |
| Cashier / POS | `cashier1` | `1234` |
| Kitchen Staff | `kitchen1` | `kitchen` |

## Documentation
For an in-depth architectural explanation and a guide on how to extend the codebase, please read the [TUTORIAL.md](./TUTORIAL.md) file.
