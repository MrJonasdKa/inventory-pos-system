namespace InventoryPOS.Core.Models;

public class ProductListItem
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int ReorderThreshold { get; set; }
    public int QuantityOnHand { get; set; }
}
