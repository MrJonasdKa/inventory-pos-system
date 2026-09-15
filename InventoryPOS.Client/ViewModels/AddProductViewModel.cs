using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;

namespace InventoryPOS.Client.ViewModels;

public partial class AddProductViewModel : ObservableObject
{
    private readonly IProductRepository _productRepository;

    [ObservableProperty] private string sku = string.Empty;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string unitPrice = string.Empty;
    [ObservableProperty] private string costPrice = string.Empty;
    [ObservableProperty] private string reorderThreshold = string.Empty;
    [ObservableProperty] private string errorMessage = string.Empty;

    public bool Saved { get; private set; }
    public event Action? CloseRequested;

    public AddProductViewModel(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Sku) || string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "SKU and Name are required.";
            return;
        }

        if (!decimal.TryParse(UnitPrice, out var unitPriceValue) || unitPriceValue < 0)
        {
            ErrorMessage = "Enter a valid unit price.";
            return;
        }

        if (!decimal.TryParse(CostPrice, out var costPriceValue) || costPriceValue < 0)
        {
            ErrorMessage = "Enter a valid cost price.";
            return;
        }

        if (!int.TryParse(ReorderThreshold, out var reorderValue) || reorderValue < 0)
        {
            ErrorMessage = "Enter a valid reorder threshold.";
            return;
        }

        try
        {
            await _productRepository.CreateAsync(new Product
            {
                Sku = Sku.Trim(),
                Name = Name.Trim(),
                UnitPrice = unitPriceValue,
                CostPrice = costPriceValue,
                ReorderThreshold = reorderValue,
                IsActive = true
            });

            Saved = true;
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save: {ex.Message}";
        }
    }
}
