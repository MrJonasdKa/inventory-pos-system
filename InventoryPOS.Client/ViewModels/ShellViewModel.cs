using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryPOS.Core.Interfaces;

namespace InventoryPOS.Client.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly ICurrentSession _currentSession;

    [ObservableProperty]
    private object? currentView;

    public string LoggedInUserName => _currentSession.CurrentUser?.FullName ?? "Unknown";

    public ShellViewModel(INavigationService navigationService, ICurrentSession currentSession)
    {
        _navigationService = navigationService;
        _currentSession = currentSession;
        _navigationService.CurrentViewChanged += vm => CurrentView = vm;
    }

    [RelayCommand]
    private void NavigateToInventory() => _navigationService.NavigateTo<PlaceholderViewModel>();

    [RelayCommand]
    private void NavigateToPos() => _navigationService.NavigateTo<PlaceholderViewModel>();

    [RelayCommand]
    private void NavigateToReports() => _navigationService.NavigateTo<PlaceholderViewModel>();
}
