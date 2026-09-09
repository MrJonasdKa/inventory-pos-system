namespace InventoryPOS.Core.Models;

public class StockLevel
{
    public int ProductId { get; set; }
    public int QuantityOnHand { get; set; }
    public DateTime UpdatedAt { get; set; }
}
