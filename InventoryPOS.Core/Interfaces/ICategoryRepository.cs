using InventoryPOS.Core.Models;

namespace InventoryPOS.Core.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<int> CreateAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id);
}
