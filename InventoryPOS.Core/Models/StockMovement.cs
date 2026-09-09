namespace InventoryPOS.Core.Models;

public enum MovementType
{
    Purchase,
    Sale,
    Adjustment,
    Return
}

public class StockMovement
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public MovementType MovementType { get; set; }
    public int QuantityChange { get; set; }
    public string? Note { get; set; }
    public int PerformedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
