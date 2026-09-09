using InventoryPOS.Core.Models;

namespace InventoryPOS.Core.Interfaces;

public interface IStockMovementRepository
{
    Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId);
    Task<long> CreateAsync(StockMovement movement);
}
