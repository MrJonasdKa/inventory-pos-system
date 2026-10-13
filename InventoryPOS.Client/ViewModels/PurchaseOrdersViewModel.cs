using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryPOS.Client.Models;
using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;

namespace InventoryPOS.Client.ViewModels;

public partial class PurchaseOrdersViewModel : ObservableObject
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPurchaseOrderRepository _poRepository;
    private readonly ICurrentSession _currentSession;

    [ObservableProperty] private ObservableCollection<Supplier> suppliers = new();
    [ObservableProperty] private Supplier? selectedSupplier;
    [ObservableProperty] private ObservableCollection<Product> availableProducts = new();
    [ObservableProperty] private Product? selectedProduct;
    [ObservableProperty] private string quantityInput = "1";
    [ObservableProperty] private string unitCostInput = "0";
    [ObservableProperty] private ObservableCollection<POCartItem> poCart = new();
    [ObservableProperty] private ObservableCollection<PurchaseOrder> pendingOrders = new();
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private string successMessage = string.Empty;

    public PurchaseOrdersViewModel(
        ISupplierRepository supplierRepository, IProductRepository productRepository,
        IPurchaseOrderRepository poRepository, ICurrentSession currentSession)
    {
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _poRepository = poRepository;
        _currentSession = currentSession;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        Suppliers = new ObservableCollection<Supplier>(await _supplierRepository.GetAllAsync());
        AvailableProducts = new ObservableCollection<Product>(await _productRepository.GetAllAsync());
        PendingOrders = new ObservableCollection<PurchaseOrder>(await _poRepository.GetPendingAsync());
    }

    [RelayCommand]
    private void AddToPoCart()
    {
        ErrorMessage = string.Empty;

        if (SelectedProduct == null) { ErrorMessage = "Select a product."; return; }
        if (!int.TryParse(QuantityInput, out var qty) || qty <= 0) { ErrorMessage = "Enter a valid quantity."; return; }
        if (!decimal.TryParse(UnitCostInput, out var cost) || cost < 0) { ErrorMessage = "Enter a valid unit cost."; return; }

        PoCart.Add(new POCartItem
        {
            ProductId = SelectedProduct.Id,
            Name = SelectedProduct.Name,
            QuantityOrdered = qty,
            UnitCost = cost
        });
    }

    [RelayCommand]
    private async Task CreatePurchaseOrderAsync()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (SelectedSupplier == null) { ErrorMessage = "Select a supplier."; return; }
        if (PoCart.Count == 0) { ErrorMessage = "Add at least one item."; return; }
        if (_currentSession.CurrentUser == null) { ErrorMessage = "No user signed in."; return; }

        try
        {
            var items = PoCart.Select(i => new PurchaseOrderItemInput
            {
                ProductId = i.ProductId,
                QuantityOrdered = i.QuantityOrdered,
                UnitCost = i.UnitCost
            }).ToList();

            var order = await _poRepository.CreateAsync(SelectedSupplier.Id, _currentSession.CurrentUser.Id, items);
            SuccessMessage = $"Purchase order #{order.Id} created.";
            PoCart.Clear();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create order: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ReceiveOrderAsync(PurchaseOrder order)
    {
        if (_currentSession.CurrentUser == null) { ErrorMessage = "No user signed in."; return; }

        try
        {
            await _poRepository.ReceiveAsync(order.Id, _currentSession.CurrentUser.Id);
            SuccessMessage = $"Purchase order #{order.Id} received — stock updated.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to receive order: {ex.Message}";
        }
    }
}
