using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;

namespace InventoryPOS.Core.Services;

public class CurrentSession : ICurrentSession
{
    public User? CurrentUser { get; private set; }

    public void SignIn(User user) => CurrentUser = user;

    public void SignOut() => CurrentUser = null;
}
