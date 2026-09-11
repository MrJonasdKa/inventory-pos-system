namespace InventoryPOS.Core.Interfaces;

public interface INavigationService
{
    event Action<object>? CurrentViewChanged;
    void NavigateTo<TViewModel>() where TViewModel : class;
}
