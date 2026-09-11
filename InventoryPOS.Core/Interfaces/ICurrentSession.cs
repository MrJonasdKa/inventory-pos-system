using InventoryPOS.Core.Models;

namespace InventoryPOS.Core.Interfaces;

public interface ICurrentSession
{
    User? CurrentUser { get; }
    void SignIn(User user);
    void SignOut();
}
