using System.Windows;
using System.Windows.Controls;
using InventoryPOS.Client.ViewModels;

namespace InventoryPOS.Client.Views;

public partial class POSView : UserControl
{
    public POSView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is POSViewModel vm)
            await vm.LoadCommand.ExecuteAsync(null);
    }
}
