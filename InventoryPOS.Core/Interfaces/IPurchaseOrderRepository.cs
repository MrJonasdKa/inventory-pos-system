using InventoryPOS.Core.Models;

namespace InventoryPOS.Core.Interfaces;

public class PurchaseOrderItemInput
{
    public int ProductId { get; set; }
    public int QuantityOrdered { get; set; }
    public decimal UnitCost { get; set; }
}

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder> CreateAsync(int supplierId, int createdByUserId, List<PurchaseOrderItemInput> items);
    Task<PurchaseOrder?> GetByIdAsync(int id);
    Task<IEnumerable<PurchaseOrderItem>> GetItemsByOrderIdAsync(int purchaseOrderId);
    Task ReceiveAsync(int purchaseOrderId, int receivedByUserId);
}
