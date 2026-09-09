using InventoryPOS.Core.Models;

namespace InventoryPOS.Core.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<int> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeactivateAsync(int id);
}
