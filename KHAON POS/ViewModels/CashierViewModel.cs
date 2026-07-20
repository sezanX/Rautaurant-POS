using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KHAONPOS.Data.Entities;
using KHAONPOS.Services;

namespace KHAONPOS.ViewModels;

public class CashierViewModel : BaseViewModel
{
    private readonly IInventoryService _inventoryService;
    private readonly IOrderService _orderService;
    private readonly IReportingService _reportingService;

    public ObservableCollection<Category> Categories { get; } = [];
    public ObservableCollection<MenuItem> MenuItems { get; } = [];

    private Category? _selectedCategory;
    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    private Order? _currentOrder;
    public Order? CurrentOrder
    {
        get => _currentOrder;
        set
        {
            if (SetProperty(ref _currentOrder, value))
            {
                RefreshCartItems();
                OnPropertyChanged(nameof(TotalAmount));
            }
        }
    }

    private void RefreshCartItems()
    {
        CartItems.Clear();
        if (_currentOrder != null)
        {
            foreach (var item in _currentOrder.OrderItems)
            {
                CartItems.Add(item);
            }
        }
    }

    public ObservableCollection<OrderItem> CartItems { get; } = [];

    public decimal TotalAmount => _currentOrder?.TotalAmount ?? 0;

    public int EstimatedTime => CartItems.Sum(oi => (oi.MenuItem?.PreparationTimeMinutes ?? 0) * oi.Quantity);

    public ICommand SelectCategoryCommand { get; }
    public ICommand AddToCartCommand { get; }
    public ICommand RemoveFromCartCommand { get; }
    public ICommand IncreaseQuantityCommand { get; }
    public ICommand DecreaseQuantityCommand { get; }
    public ICommand UpdateExtraChargeCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand CheckoutCommand { get; }
    public ICommand PrintReceiptCommand { get; }
    public ICommand LoadDataCommand { get; }

    private readonly SemaphoreSlim _orderLock = new(1, 1);

    public CashierViewModel(IInventoryService inventoryService, IOrderService orderService, IReportingService reportingService)
    {
        _inventoryService = inventoryService;
        _orderService = orderService;
        _reportingService = reportingService;

        SelectCategoryCommand = new RelayCommand<Category>(async c =>
        {
            SelectedCategory = c;
            await LoadMenuItems(c?.Id);
        });
        AddToCartCommand = new RelayCommand<MenuItem>(async m => await ExecuteOrderAction(() => AddToCart(m)));
        RemoveFromCartCommand = new RelayCommand<OrderItem>(async o => await ExecuteOrderAction(() => RemoveFromCart(o)));
        IncreaseQuantityCommand = new RelayCommand<OrderItem>(async o => await ExecuteOrderAction(() => UpdateQuantity(o, 1)));
        DecreaseQuantityCommand = new RelayCommand<OrderItem>(async o => await ExecuteOrderAction(() => UpdateQuantity(o, -1)));
        UpdateExtraChargeCommand = new RelayCommand<OrderItem>(async o => await ExecuteOrderAction(() => UpdateExtraCharge(o)));
        ClearAllCommand = new RelayCommand(async _ => await ExecuteOrderAction(ClearAll));
        CheckoutCommand = new RelayCommand(async _ => await ExecuteOrderAction(PlaceOrder), _ => _currentOrder?.OrderItems.Count > 0);
        PrintReceiptCommand = new RelayCommand(async _ => await ExecuteOrderAction(PrintReceipt), _ => _currentOrder != null);
        LoadDataCommand = new RelayCommand(async _ => await InitializeAsync());

        // Load data on initialization
        _ = InitializeAsync();
    }

    private async Task ExecuteOrderAction(Func<Task> action)
    {
        await _orderLock.WaitAsync();
        try
        {
            await action();
        }
        finally
        {
            _orderLock.Release();
        }
    }

    public async Task InitializeAsync()
    {
        try
        {
            var categories = await _inventoryService.GetCategoriesAsync();
            Categories.Clear();
            Categories.Add(new Category { Id = 0, Name = "All", IconName = "SilverwareForkKnife" });
            foreach (var c in categories) Categories.Add(c);

            var items = await _inventoryService.GetMenuItemsAsync();
            MenuItems.Clear();
            foreach (var i in items) MenuItems.Add(i);

            // Create a new order
            var userId = LoginViewModel.CurrentUser?.Id ?? 2;
            CurrentOrder = await _orderService.CreateOrderAsync(userId);

            // Default to "All" category selected
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == 0);
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message ?? "No inner exception";
            System.Windows.MessageBox.Show($"InitializeAsync error: {ex.Message}\nInner: {inner}\n{ex.StackTrace}");
        }
    }

    private async Task LoadMenuItems(int? categoryId)
    {
        if (categoryId == null) return;

        System.Collections.Generic.List<MenuItem> items;
        if (categoryId.Value == 0)
        {
            items = await _inventoryService.GetMenuItemsAsync();
        }
        else
        {
            items = await _inventoryService.GetMenuItemsByCategoryAsync(categoryId.Value);
        }

        MenuItems.Clear();
        foreach (var i in items) MenuItems.Add(i);
    }

    private async Task AddToCart(MenuItem? item)
    {
        if (item == null || _currentOrder == null) return;

        CurrentOrder = await _orderService.AddItemToOrderAsync(_currentOrder.Id, item.Id, 1);

        // Force update since EF Core tracking might return the exact same Order instance
        // which prevents SetProperty from triggering the update
        RefreshCartItems();
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(EstimatedTime));
        System.Windows.Input.CommandManager.InvalidateRequerySuggested(); // Force re-eval of canExecute
    }

    private async Task RemoveFromCart(OrderItem? item)
    {
        if (item == null || _currentOrder == null) return;

        await _orderService.RemoveItemFromOrderAsync(item.Id);
        CurrentOrder = await _orderService.GetOrderByIdAsync(_currentOrder.Id);
        RefreshCartItems();
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(EstimatedTime));
    }

    private async Task UpdateQuantity(OrderItem? item, int change)
    {
        if (item == null || _currentOrder == null) return;

        int newQuantity = item.Quantity + change;
        if (newQuantity <= 0)
        {
            await _orderService.RemoveItemFromOrderAsync(item.Id);
        }
        else
        {
            await _orderService.UpdateOrderItemQuantityAsync(item.Id, newQuantity);
            // Save remarks if they were edited
            if (item.Remarks != null)
            {
                await _orderService.UpdateOrderItemRemarksAsync(item.Id, item.Remarks);
            }
        }

        CurrentOrder = await _orderService.GetOrderByIdAsync(_currentOrder.Id);
        RefreshCartItems();
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(EstimatedTime));
    }

    private async Task UpdateExtraCharge(OrderItem? item)
    {
        if (item == null || _currentOrder == null) return;

        await _orderService.UpdateOrderItemExtraChargeAsync(item.Id, item.ExtraCharge);

        CurrentOrder = await _orderService.GetOrderByIdAsync(_currentOrder.Id);
        RefreshCartItems();
        OnPropertyChanged(nameof(TotalAmount));
    }

    private async Task ClearAll()
    {
        if (_currentOrder == null) return;
        var items = _currentOrder.OrderItems.ToList();
        foreach (var item in items)
        {
            await _orderService.RemoveItemFromOrderAsync(item.Id);
        }
        CurrentOrder = await _orderService.GetOrderByIdAsync(_currentOrder.Id);
        RefreshCartItems();
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(EstimatedTime));
    }

    private async Task PlaceOrder()
    {
        if (_currentOrder == null) return;

        // Save any pending remarks
        foreach (var item in _currentOrder.OrderItems)
        {
            if (item.Remarks != null)
            {
                await _orderService.UpdateOrderItemRemarksAsync(item.Id, item.Remarks);
            }
        }

        await _orderService.PlaceOrderAsync(_currentOrder.Id, EstimatedTime);
        CurrentOrder = await _orderService.GetOrderByIdAsync(_currentOrder.Id);

        // Start a new order
        var userId = LoginViewModel.CurrentUser?.Id ?? 2;
        CurrentOrder = await _orderService.CreateOrderAsync(userId);

        // Reset category selection to "All"
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == 0);
        await LoadMenuItems(0);
    }

    private async Task PrintReceipt()
    {
        if (_currentOrder == null || _currentOrder.OrderItems.Count == 0) return;

        try
        {
            // Generate 80mm PDF via QuestPDF and open it
            var pdfPath = await _reportingService.GenerateReceiptPdfAsync(_currentOrder);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(pdfPath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error printing receipt: {ex.Message}");
        }
    }
}
