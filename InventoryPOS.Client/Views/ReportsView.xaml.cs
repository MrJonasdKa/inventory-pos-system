using System.Windows;
using System.Windows.Controls;
using InventoryPOS.Client.ViewModels;

namespace InventoryPOS.Client.Views;

public partial class ReportsView : UserControl
{
    public ReportsView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is InventoryViewModel vm)
            await vm.LoadCommand.ExecuteAsync(null);
    }
}
