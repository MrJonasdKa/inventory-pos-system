using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryPOS.Core.Interfaces;

namespace InventoryPOS.Client.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly ICurrentSession _currentSession;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public bool LoginSucceeded { get; private set; }

    public LoginViewModel(IAuthService authService, ICurrentSession currentSession)
    {
        _authService = authService;
        _currentSession = currentSession;
    }

    [RelayCommand]
    private async Task LoginAsync(object parameter)
    {
        ErrorMessage = string.Empty;

        // Password comes from the PasswordBox via code-behind (see below) —
        // PasswordBox can't be data-bound directly for security reasons.
        var password = parameter as string ?? string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Enter both username and password.";
            return;
        }

        var user = await _authService.LoginAsync(Username, password);

        if (user == null)
        {
            ErrorMessage = "Invalid username or password.";
            return;
        }

        _currentSession.SignIn(user);
        LoginSucceeded = true;

        CloseRequested?.Invoke();
    }

    public event Action? CloseRequested;
}
