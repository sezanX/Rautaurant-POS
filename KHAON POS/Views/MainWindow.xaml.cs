using System.Windows;
using KHAONPOS.ViewModels;

namespace KHAONPOS.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
