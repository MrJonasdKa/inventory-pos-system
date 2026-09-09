using InventoryPOS.Core.Models;

namespace InventoryPOS.Core.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetBySkuAsync(string sku);
    Task<int> CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeactivateAsync(int id);
}
