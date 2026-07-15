using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RestaurantPOS.Data.Entities;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels;

public class PosViewModel : BaseViewModel
{
    private readonly IInventoryService _inventoryService;
    private readonly IOrderService _orderService;
    private readonly IReportingService _reportingService;

    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<MenuItem> MenuItems { get; } = new();
    
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

    public ObservableCollection<OrderItem> CartItems { get; } = new();

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

    public PosViewModel(IInventoryService inventoryService, IOrderService orderService, IReportingService reportingService)
    {
        _inventoryService = inventoryService;
        _orderService = orderService;
        _reportingService = reportingService;

        SelectCategoryCommand = new RelayCommand<Category>(async c => await LoadMenuItems(c?.Id));
        AddToCartCommand = new RelayCommand<MenuItem>(async m => await AddToCart(m));
        RemoveFromCartCommand = new RelayCommand<OrderItem>(async o => await RemoveFromCart(o));
        IncreaseQuantityCommand = new RelayCommand<OrderItem>(async o => await UpdateQuantity(o, 1));
        DecreaseQuantityCommand = new RelayCommand<OrderItem>(async o => await UpdateQuantity(o, -1));
        UpdateExtraChargeCommand = new RelayCommand<OrderItem>(async o => await UpdateExtraCharge(o));
        ClearAllCommand = new RelayCommand(async _ => await ClearAll());
        CheckoutCommand = new RelayCommand(async _ => await PlaceOrder(), _ => _currentOrder != null && _currentOrder.OrderItems.Any());
        PrintReceiptCommand = new RelayCommand(async _ => await PrintReceipt(), _ => _currentOrder != null);
        LoadDataCommand = new RelayCommand(async _ => await InitializeAsync());
        
        // Load data on initialization
        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        var categories = await _inventoryService.GetCategoriesAsync();
        Categories.Clear();
        Categories.Add(new Category { Id = 0, Name = "All", IconName = "SilverwareForkKnife" });
        foreach (var c in categories) Categories.Add(c);

        var items = await _inventoryService.GetMenuItemsAsync();
        MenuItems.Clear();
        foreach (var i in items) MenuItems.Add(i);

        // Create a new order
        CurrentOrder = await _orderService.CreateOrderAsync(null, 2); // Hardcoded userId 2 (Cashier)
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
        CurrentOrder = await _orderService.CreateOrderAsync(null, 2);
    }

    private async Task PrintReceipt()
    {
        if (_currentOrder == null || !_currentOrder.OrderItems.Any()) return;

        var printDialog = new System.Windows.Controls.PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            var doc = new System.Windows.Documents.FlowDocument();
            doc.PagePadding = new System.Windows.Thickness(50);
            doc.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");

            var title = new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run("KHAON POS RECEIPT"))
            {
                FontSize = 24,
                FontWeight = System.Windows.FontWeights.Bold,
                TextAlignment = System.Windows.TextAlignment.Center
            };
            doc.Blocks.Add(title);

            var orderInfo = new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run($"Order #{_currentOrder.Id}\nDate: {DateTime.Now}"))
            {
                TextAlignment = System.Windows.TextAlignment.Center
            };
            doc.Blocks.Add(orderInfo);

            doc.Blocks.Add(new System.Windows.Documents.BlockUIContainer(new System.Windows.Controls.Separator()));

            var itemsList = new System.Windows.Documents.Paragraph();
            foreach (var item in _currentOrder.OrderItems)
            {
                itemsList.Inlines.Add(new System.Windows.Documents.Run($"{item.Quantity}x {item.MenuItem?.Name ?? "Item"}  -  {item.TotalPrice:C}\n") { FontWeight = System.Windows.FontWeights.Bold });
                
                if (!string.IsNullOrEmpty(item.Remarks) || item.ExtraCharge > 0)
                {
                    string details = "";
                    if (!string.IsNullOrEmpty(item.Remarks)) details += $"  Note: {item.Remarks}\n";
                    if (item.ExtraCharge > 0) details += $"  Extra Charge: {item.ExtraCharge:C} per item\n";
                    itemsList.Inlines.Add(new System.Windows.Documents.Run(details) { FontSize = 12, Foreground = System.Windows.Media.Brushes.Gray });
                }
            }
            doc.Blocks.Add(itemsList);

            doc.Blocks.Add(new System.Windows.Documents.BlockUIContainer(new System.Windows.Controls.Separator()));

            var total = new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run($"Total: {_currentOrder.TotalAmount:C}"))
            {
                FontSize = 18,
                FontWeight = System.Windows.FontWeights.Bold,
                TextAlignment = System.Windows.TextAlignment.Right
            };
            doc.Blocks.Add(total);

            System.Windows.Documents.IDocumentPaginatorSource idpSource = doc;
            printDialog.PrintDocument(idpSource.DocumentPaginator, $"Receipt Order #{_currentOrder.Id}");
        }
        await Task.CompletedTask;
    }
}
