using System.Windows;
using InventoryPOS.Client.ViewModels;

namespace InventoryPOS.Client;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        _viewModel.CloseRequested += () => DialogResult = true;
    }

    private async void SignInButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoginCommand.ExecuteAsync(PasswordBox.Password);
    }

    public bool LoginSucceeded => _viewModel.LoginSucceeded;
}
