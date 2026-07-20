using System;
using System.Diagnostics;
using System.Windows.Input;

namespace KHAONPOS.ViewModels;

public class MainViewModel : BaseViewModel
{
    private BaseViewModel _currentViewModel;
    private readonly IServiceProvider _serviceProvider;

    public BaseViewModel CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            if (SetProperty(ref _currentViewModel, value))
            {
                OnPropertyChanged(nameof(IsLoggedIn));
            }
        }
    }

    public bool IsLoggedIn => CurrentViewModel is not LoginViewModel;

    public ICommand NavigateCommand { get; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "WPF INotifyPropertyChanged requires instance properties.")]
    public DateTime CurrentDate => DateTime.Now;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "WPF INotifyPropertyChanged requires instance properties.")]
    public DateTime CurrentTime => DateTime.Now;

    private readonly System.Windows.Threading.DispatcherTimer _timer;

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _currentViewModel = (BaseViewModel)_serviceProvider.GetService(typeof(LoginViewModel))!;

        NavigateCommand = new RelayCommand<string>(Navigate);

        // Subscribe to a static login event to change to POS view on success
        LoginViewModel.LoginSuccessful += OnLoginSuccessful;

        _timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, e) =>
        {
            OnPropertyChanged(nameof(CurrentDate));
            OnPropertyChanged(nameof(CurrentTime));
        };
        _timer.Start();
    }

    private void Navigate(string? viewName)
    {
        if (string.IsNullOrEmpty(viewName)) return;

        switch (viewName)
        {
            case "POS":
                CurrentViewModel = (BaseViewModel)_serviceProvider.GetService(typeof(CashierViewModel))!;
                break;
            case "Kitchen":
                CurrentViewModel = (BaseViewModel)_serviceProvider.GetService(typeof(KitchenViewModel))!;
                break;
            case "Dashboard":
                CurrentViewModel = (BaseViewModel)_serviceProvider.GetService(typeof(AdminDashboardViewModel))!;
                break;
            case "Support":
                OpenSupportLink();
                break;
            case "Logout":
                CurrentViewModel = (BaseViewModel)_serviceProvider.GetService(typeof(LoginViewModel))!;
                break;
        }
    }

    private static void OpenSupportLink()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/sezanX/Rautaurant-POS",
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore failures; user can open support manually if system policy blocks shell execute.
        }
    }

    private void OnLoginSuccessful(object? sender, string role)
    {
        if (role == "Kitchen")
        {
            Navigate("Kitchen");
        }
        else if (role == "Admin")
        {
            Navigate("Dashboard");
        }
        else
        {
            Navigate("POS");
        }
    }
}
