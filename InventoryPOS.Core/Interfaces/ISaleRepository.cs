using InventoryPOS.Core.Models;

namespace InventoryPOS.Core.Interfaces;

public class SaleItemInput
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public interface ISaleRepository
{
    Task<Sale> CompleteSaleAsync(int cashierId, PaymentMethod paymentMethod, decimal discountAmount, List<SaleItemInput> items);
    Task<Sale?> GetByIdAsync(long id);
    Task<IEnumerable<SaleItem>> GetItemsBySaleIdAsync(long saleId);
}
