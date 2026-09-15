using System.Windows;
using InventoryPOS.Client.ViewModels;

namespace InventoryPOS.Client;

public partial class AddProductWindow : Window
{
    private readonly AddProductViewModel _viewModel;

    public AddProductWindow(AddProductViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        _viewModel.CloseRequested += () => DialogResult = true;
    }
}
