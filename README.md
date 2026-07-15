# KHAON POS

A production-ready Desktop Restaurant Point-of-Sale (POS) application built on modern Microsoft technologies.

![KHAON POS Logo](./logo.png)

## Tech Stack
- **Framework**: .NET 8
- **UI Framework**: Windows Presentation Foundation (WPF)
- **UI Library**: Material Design in XAML
- **Architecture**: Strict MVVM (Model-View-ViewModel)
- **Database**: PostgreSQL (Cloud-hosted via Neon.tech for real-time multi-user sync)
- **ORM**: Entity Framework Core 8 (Npgsql)
- **Reporting**: WPF FlowDocument Printing & QuestPDF
- **Barcode & QR**: ZXing.Net
- **Charts & Analytics**: LiveCharts2 (LiveChartsCore.SkiaSharpView.WPF)
- **Dependency Injection**: Microsoft.Extensions.Hosting

## Features
- **Real-Time Multi-User Cloud Database**: Seamlessly syncs orders and menu items between Cashiers, Kitchen Staff, and Admins across different computers in real-time.
- **Role-Based Authentication**: Separate views for Admin, Cashier, and Kitchen staff.
- **Dynamic POS Interface**: Category-filtered menu items, shopping cart, custom remarks, and custom extra charges.
- **Receipt Printing**: Generates and prints beautiful WPF FlowDocument receipts featuring itemized lists, remarks, and extra charges.
- **Kitchen Kanban Board**: Live auto-refreshing board showing active orders and their preparation status.
- **Admin Dashboard**: Real-time sales KPI tracking and a visual trend chart with an interactive navigation sidebar.
- **Automated Database Seeding**: Automatic EF Core database creation and population on startup to ensure a smooth onboarding experience.

## Prerequisites
To compile or run the source code, you must have the following installed on your Windows machine:
1. [**Visual Studio 2022**](https://visualstudio.microsoft.com/) (Recommended) OR the [**.NET 8.0 SDK**](https://dotnet.microsoft.com/download/dotnet/8.0).
2. An active internet connection (to connect to the cloud PostgreSQL database).

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
