using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using RestaurantPOS.Data.Entities;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels;

public class KitchenOrderViewModel : BaseViewModel
{
    private readonly Order _order;
    public Order Order => _order;

    public KitchenOrderViewModel(Order order)
    {
        _order = order;
    }

    public string TimeRemaining
    {
        get
        {
            if (_order.EstimatedCompletionTime.HasValue)
            {
                var diff = _order.EstimatedCompletionTime.Value - DateTime.Now;
                if (diff.TotalSeconds < 0) return "00:00 min";
                return $"{(int)diff.TotalMinutes:D2}:{diff.Seconds:D2} min";
            }
            return "--:-- min";
        }
    }

    public void RefreshTime()
    {
        OnPropertyChanged(nameof(TimeRemaining));
    }
}

public class KitchenViewModel : BaseViewModel
{
    private readonly IOrderService _orderService;
    private DispatcherTimer _timer;

    public ObservableCollection<KitchenOrderViewModel> ActiveOrders { get; } = new();

    public ICommand MarkAsPreparedCommand { get; }
    public ICommand AddTimeCommand { get; }
    public ICommand RefreshCommand { get; }
    
    public string CurrentTime => DateTime.Now.ToString("h:mm tt, ddd, MMM d, yyyy");

    public KitchenViewModel(IOrderService orderService)
    {
        _orderService = orderService;

        MarkAsPreparedCommand = new RelayCommand<KitchenOrderViewModel>(async o => await MarkAsPrepared(o));
        AddTimeCommand = new RelayCommand<KitchenOrderViewModel>(async o => await AddTime(o, 5));
        RefreshCommand = new RelayCommand(async _ => await LoadActiveOrders());

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, e) => 
        {
            OnPropertyChanged(nameof(CurrentTime));
            foreach (var order in ActiveOrders)
            {
                order.RefreshTime();
            }
        };
        _timer.Start();

        // Separate timer for polling the db
        var dbTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
        dbTimer.Tick += async (s, e) => await LoadActiveOrders();
        dbTimer.Start();

        _ = LoadActiveOrders();
    }

    private async Task LoadActiveOrders()
    {
        var orders = await _orderService.GetActiveOrdersAsync();
        
        var existingIds = ActiveOrders.Select(o => o.Order.Id).ToList();
        var newOrders = orders.Where(o => (o.Status == "Pending" || o.Status == "Preparing")).ToList();
        
        // Remove completed or removed orders
        var toRemove = ActiveOrders.Where(o => !newOrders.Any(no => no.Id == o.Order.Id)).ToList();
        foreach(var rm in toRemove) ActiveOrders.Remove(rm);

        // Add new orders
        foreach (var order in newOrders)
        {
            var existing = ActiveOrders.FirstOrDefault(o => o.Order.Id == order.Id);
            if (existing == null)
            {
                ActiveOrders.Add(new KitchenOrderViewModel(order));
            }
            else
            {
                // update existing order if needed (maybe new items) - simplest is to just re-wrap
                var index = ActiveOrders.IndexOf(existing);
                ActiveOrders[index] = new KitchenOrderViewModel(order);
            }
        }
    }

    private async Task MarkAsPrepared(KitchenOrderViewModel? kOrder)
    {
        if (kOrder == null) return;
        await _orderService.UpdateOrderStatusAsync(kOrder.Order.Id, "Served"); // or "Prepared"
        ActiveOrders.Remove(kOrder);
    }
    
    private async Task AddTime(KitchenOrderViewModel? kOrder, int minutes)
    {
        if (kOrder == null) return;
        await _orderService.AddTimeToOrderAsync(kOrder.Order.Id, minutes);
        await LoadActiveOrders(); // Reload to get updated time
    }
}
