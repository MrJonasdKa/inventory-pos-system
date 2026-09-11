using CommunityToolkit.Mvvm.ComponentModel;

namespace InventoryPOS.Client.ViewModels;

public partial class PlaceholderViewModel : ObservableObject
{
    [ObservableProperty]
    private string message = "This screen hasn't been built yet.";
}
