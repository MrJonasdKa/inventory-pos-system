using InventoryPOS.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryPOS.Client.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public event Action<object>? CurrentViewChanged;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        CurrentViewChanged?.Invoke(viewModel);
    }
}
