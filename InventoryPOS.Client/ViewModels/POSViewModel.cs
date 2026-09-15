using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryPOS.Client.Models;
using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;

namespace InventoryPOS.Client.ViewModels;

public partial class POSViewModel : ObservableObject
{
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly ICurrentSession _currentSession;

    [ObservableProperty]
    private ObservableCollection<Product> availableProducts = new();

    [ObservableProperty]
    private Product? selectedProduct;

    [ObservableProperty]
    private string quantityInput = "1";

    [ObservableProperty]
    private ObservableCollection<CartItem> cart = new();

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string successMessage = string.Empty;

    public decimal Total => Cart.Sum(item => item.LineTotal);

    public POSViewModel(IProductRepository productRepository, ISaleRepository saleRepository, ICurrentSession currentSession)
    {
        _productRepository = productRepository;
        _saleRepository = saleRepository;
        _currentSession = currentSession;
        Cart.CollectionChanged += (s, e) => OnPropertyChanged(nameof(Total));
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        ErrorMessage = string.Empty;
        var result = await _productRepository.GetAllAsync();
        AvailableProducts = new ObservableCollection<Product>(result);
    }

    [RelayCommand]
    private void AddToCart()
    {
        ErrorMessage = string.Empty;

        if (SelectedProduct == null)
        {
            ErrorMessage = "Select a product first.";
            return;
        }

        if (!int.TryParse(QuantityInput, out var quantity) || quantity <= 0)
        {
            ErrorMessage = "Enter a valid quantity.";
            return;
        }

        var existing = Cart.FirstOrDefault(c => c.ProductId == SelectedProduct.Id);
        if (existing != null)
        {
            existing.Quantity += quantity;
            // Force the grid to refresh this row's computed LineTotal
            var index = Cart.IndexOf(existing);
            Cart.RemoveAt(index);
            Cart.Insert(index, existing);
        }
        else
        {
            Cart.Add(new CartItem
            {
                ProductId = SelectedProduct.Id,
                Sku = SelectedProduct.Sku,
                Name = SelectedProduct.Name,
                UnitPrice = SelectedProduct.UnitPrice,
                Quantity = quantity
            });
        }

        OnPropertyChanged(nameof(Total));
        QuantityInput = "1";
    }

    [RelayCommand]
    private void RemoveFromCart(CartItem item)
    {
        Cart.Remove(item);
        OnPropertyChanged(nameof(Total));
    }

    [RelayCommand]
    private async Task CompleteSaleAsync()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (Cart.Count == 0)
        {
            ErrorMessage = "Cart is empty.";
            return;
        }

        if (_currentSession.CurrentUser == null)
        {
            ErrorMessage = "No user is signed in.";
            return;
        }

        try
        {
            var items = Cart.Select(c => new SaleItemInput
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity
            }).ToList();

            var sale = await _saleRepository.CompleteSaleAsync(
                _currentSession.CurrentUser.Id, PaymentMethod.Cash, discountAmount: 0, items);

            SuccessMessage = $"Sale {sale.SaleNumber} completed — total {sale.TotalAmount:C}";
            Cart.Clear();
            OnPropertyChanged(nameof(Total));
        }
        catch (Exception ex)
        {
            // This is where insufficient-stock or product-not-found errors from
            // SaleRepository surface directly to the cashier.
            ErrorMessage = $"Sale failed: {ex.Message}";
        }
    }
}
