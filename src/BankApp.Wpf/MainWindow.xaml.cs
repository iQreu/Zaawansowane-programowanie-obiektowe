using System.Windows;
using BankApp.Wpf.ViewModels;

namespace BankApp.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
