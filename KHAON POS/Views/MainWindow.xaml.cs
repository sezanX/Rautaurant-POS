using System.Windows;
using RestaurantPOS.ViewModels;

namespace RestaurantPOS.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
