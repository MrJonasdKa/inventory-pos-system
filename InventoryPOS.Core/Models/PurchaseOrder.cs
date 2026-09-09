namespace InventoryPOS.Core.Models;

public enum PurchaseOrderStatus
{
    Pending,
    Received,
    Cancelled
}

public class PurchaseOrder
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Pending;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
