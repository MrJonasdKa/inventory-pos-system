using InventoryPOS.Core.Models;

namespace InventoryPOS.Core.Interfaces;

public interface IAuthService
{
    Task<User?> LoginAsync(string username, string password);
}
