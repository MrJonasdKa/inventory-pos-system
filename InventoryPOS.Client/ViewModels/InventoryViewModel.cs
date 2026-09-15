using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryPOS.Client.ViewModels;

public partial class InventoryViewModel : ObservableObject
{
    private readonly IProductRepository _productRepository;

    [ObservableProperty]
    private ObservableCollection<ProductListItem> products = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public InventoryViewModel(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _productRepository.GetAllWithStockAsync();
            Products = new ObservableCollection<ProductListItem>(result);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load products: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenAddProduct()
    {
        var addWindow = App.Services.GetRequiredService<AddProductWindow>();
        if (addWindow.ShowDialog() == true)
        {
            _ = LoadAsync(); // refresh the list after a successful add
        }
    }
}