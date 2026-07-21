# KHAON POS — Desktop Restaurant Point of Sale & KDS

![KHAON POS Logo](./KHAON%20POS/logo.png)

[![Framework](https://img.shields.io/badge/Framework-.NET%208.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![UI Library](https://img.shields.io/badge/UI-WPF%20%7C%20Material%20Design-0078D4?logo=windows)](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
[![Database](https://img.shields.io/badge/Database-PostgreSQL%20%7C%20EF%20Core%208-4169E1?logo=postgresql)](https://neon.tech/)
[![PDF Engine](https://img.shields.io/badge/Receipts-QuestPDF-FF6F00)](https://www.questpdf.com/)
[![Charts](https://img.shields.io/badge/Analytics-LiveCharts2-00C853)](https://livecharts.dev/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**KHAON POS** is a feature-rich, production-ready Desktop Restaurant Point-of-Sale (POS) and Kitchen Display System (KDS) application built with **C#**, **WPF (.NET 8)**, **MVVM Architecture**, and **Entity Framework Core 8** connected to a PostgreSQL database (cloud-hosted on Neon.tech or local).

Designed for modern fast-food restaurants, cafes, and eateries, KHAON POS streamlines order taking, kitchen order tracking with real-time countdown timers, receipt printing, and executive business analytics.

---

## 🌟 Key Features

### 💳 1. Cashier POS Station
- **Category Navigation & Live Search**: Filter menu items by category (*Burgers, Pizza, Drinks, Desserts, All*) or search by name/barcode.
- **Cart Management**: Add/remove items, adjust quantities, calculate subtotal and taxes dynamically.
- **Item Customizations & Extra Charges**: Add custom customer instructions per item (*e.g., "Extra Spicy", "No Onions"*) and custom extra charges (*e.g., +$1.50 for extra cheese*).
- **Automated Preparation Estimation**: Dynamically calculates order prep time based on item prep configurations.
- **80mm QuestPDF Thermal Receipt Generation**: Generates 80mm continuous thermal receipts formatted with itemized lists, custom remarks, and tax calculations, opening directly in the default system viewer/printer.

### 🍳 2. Kitchen Display System (KDS)
- **Live Ticket Board**: Real-time order queue for kitchen staff displaying pending and preparing orders.
- **Automated DB Polling**: Background synchronization every 10 seconds ensures new orders appear automatically.
- **Per-Second UI Countdown Timers**: Tracks estimated time remaining per ticket.
- **Visual Urgency Cues & System Beep Alerts**:
  - 🟢 **Green (`#66BD76`)**: On schedule (> 60 seconds remaining).
  - 🟠 **Orange (`#FF9800`)**: Cautionary window (≤ 60 seconds remaining) with an **automated system audio beep alert**.
  - 🔴 **Red (`#E53935`)**: Overdue ticket alert (elapsed time exceeded).
- **Kitchen Actions**: One-click **+5 Mins** prep delay extension and **Mark as Prepared / Served** to advance order states.

### 📊 3. Executive Admin Dashboard & Analytics
- **Executive KPI Cards**: Real-time tracking of Total Revenue, Total Orders, Average Order Value (AOV), and Active Menu Items.
- **LiveCharts2 Trend Visualization**: Interactive area and line charts for daily, weekly, and monthly sales trends.
- **Menu & Inventory Control**: Full CRUD operations for menu items, pricing, stock levels, preparation times, barcode mapping, and image URLs.
- **Category Management**: Add/edit menu categories with built-in Material Design icon picker.
- **User & Staff Control**: Role-based user management (Admin, Cashier, Kitchen Staff), password reset, and user creation.
- **Reports Export**: Printable sales reports filtered by date range.

### ☁️ 4. Cloud & Local Database Integration
- **PostgreSQL Cloud Sync**: Configured for cloud PostgreSQL (Neon.tech) out of the box using `Npgsql.EntityFrameworkCore.PostgreSQL`.
- **Automatic Auto-Migration & Data Seeding**: `AppDbContext` and `DbSeeder` automatically verify, migrate, and seed initial categories, menu items, sample orders, and user credentials on startup.

---

## 🔑 Default Login Credentials

Upon first startup, the database is automatically seeded with default accounts for each operational role:

| Role | Username | Password | Access Rights & Landing View |
| :--- | :--- | :--- | :--- |
| **Administrator** | `admin` | `admin` | Full system control: Analytics, Inventory CRUD, Category CRUD, User Management, Reports. |
| **Cashier / POS** | `cashier1` | `1234` | Order creation, shopping cart, custom remarks, checkout, QuestPDF thermal receipt printing. |
| **Kitchen Staff** | `kitchen1` | `kitchen` | Live KDS queue, countdown timers, audible warning alerts, order prep status update. |

---

## 🛠️ Architecture & Tech Stack

The application strictly adheres to the **Model-View-ViewModel (MVVM)** design pattern and uses **Dependency Injection (DI)** powered by `Microsoft.Extensions.Hosting`.

```
[ WPF XAML Views ] <---> [ ViewModels (INotifyPropertyChanged) ] <---> [ Service Layer ] <---> [ EF Core AppDbContext (Npgsql) ] <---> [ PostgreSQL ]
                                                                             │
                                                                             ├──> [ QuestPDF Engine ] (80mm Thermal Receipts)
                                                                             └──> [ LiveCharts2 ] (SkiaSharp Analytics)
```

| Component | Technology | Description |
| :--- | :--- | :--- |
| **Framework** | .NET 8.0 (`net8.0-windows`) | Modern Microsoft .NET runtime |
| **UI Framework** | WPF (Windows Presentation Foundation) | Desktop GUI engine |
| **Design System** | Material Design in XAML (v5.0) | Modern UI themes, icons, and controls |
| **Architecture** | MVVM + Dependency Injection | `Microsoft.Extensions.Hosting`, `BaseViewModel`, `RelayCommand` |
| **Database ORM** | Entity Framework Core 8.0 | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| **Database Engine** | PostgreSQL (Neon.tech Cloud / Local) | Multi-user relational database |
| **Receipt Printing** | QuestPDF (v2024.7) | 80mm continuous thermal receipt layout engine |
| **Analytics Engine** | LiveChartsCore.SkiaSharpView.WPF | Interactive sales visualization charts |
| **Logging System** | Serilog & Microsoft.Extensions.Logging | Console, debug, and `crash.log` file logging |

---

## 📁 Repository Structure

```
Rautaurant-POS-main/
├── README.md                 # Main project overview (this file)
├── TUTORIAL.md               # Architecture guide & step-by-step developer tutorial
└── KHAON POS/                # Primary WPF Application Project
    ├── KHAONPOS.csproj       # .NET 8 WPF Project & NuGet dependencies
    ├── App.xaml              # WPF Application resources & styles
    ├── App.xaml.cs           # Host builder, DI registration & startup bootstrapper
    ├── WORKFLOW.md           # System sequence, flowchart & ER diagrams (Mermaid)
    ├── logo.png              # Application logo
    ├── Data/
    │   ├── AppDbContext.cs   # EF Core DbContext & PostgreSQL configuration
    │   ├── DbSeeder.cs       # Automatic database seeder for initial data
    │   └── Entities/         # Domain entities (User, MenuItem, Order, OrderItem, Payment, Category)
    ├── Services/             # Business logic layer (OrderService, InventoryService, ReportingService)
    ├── ViewModels/           # MVVM ViewModels (MainViewModel, CashierViewModel, KitchenViewModel, Admin*ViewModels)
    ├── Views/                # WPF XAML Views & UserControls
    └── Converters/           # XAML ValueConverters (StatusToColor, Currency, RoleSelected, ActiveTab)
```

---

## 🚀 Quick Start Guide

### Prerequisites
1. **Windows OS** (Windows 10 / 11)
2. [**.NET 8.0 SDK**](https://dotnet.microsoft.com/download/dotnet/8.0) or higher installed.
3. [**Visual Studio 2022**](https://visualstudio.microsoft.com/) (with *.NET Desktop Development* workload) OR **VS Code** with C# Dev Kit.
4. Active internet connection (if connecting to the default cloud PostgreSQL database on Neon.tech).

### Option 1: Running via Visual Studio 2022
1. Clone this repository:
   ```bash
   git clone https://github.com/your-username/Rautaurant-POS-main.git
   ```
2. Open `KHAON POS/KHAONPOS.csproj` or the directory in Visual Studio 2022.
3. Press `F5` or click **Start** to build and run.

### Option 2: Running via Terminal / PowerShell
1. Open PowerShell and navigate to the project folder:
   ```powershell
   cd "d:\Software Dev Projects\Rautaurant-POS-main"
   ```
2. Build and run the project using the .NET CLI:
   ```powershell
   dotnet run --project "KHAON POS/KHAONPOS.csproj"
   ```
### For Build 
``` cd "KHAON POS"
dotnet publish -c Release -o publish
```
---

## 🗄️ Database Configuration

By default, KHAON POS connects to a cloud-hosted PostgreSQL database on Neon.tech configured in `KHAON POS/Data/AppDbContext.cs`.

To point the application to your own local or remote PostgreSQL instance:
1. Open `KHAON POS/Data/AppDbContext.cs`.
2. Update the connection string in `OnConfiguring`:
   ```csharp
   optionsBuilder.UseNpgsql("Host=YOUR_HOST;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD;");
   ```
3. Run the application; EF Core will automatically execute `Database.Migrate()` and seed the initial dataset.

---

## 📖 Further Documentation

- 📘 **[TUTORIAL.md](./TUTORIAL.md)**: Developer architecture deep-dive, layer breakdown, step-by-step feature extension guide, and database migration instructions.
---

## 📄 License

This project is open-source under the [MIT License](LICENSE).

