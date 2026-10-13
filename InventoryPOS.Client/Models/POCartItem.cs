namespace InventoryPOS.Client.Models;

public class POCartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int QuantityOrdered { get; set; }
    public decimal UnitCost { get; set; }
}
