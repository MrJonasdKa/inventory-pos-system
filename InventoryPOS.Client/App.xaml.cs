using System.IO;
using System.Windows;
using Dapper;
using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;
using InventoryPOS.Data;
using InventoryPOS.Data.Repositories;
using InventoryPOS.Core.Services;
using InventoryPOS.Client.ViewModels;
using InventoryPOS.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryPOS.Client;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        SqlMapper.AddTypeHandler(new EnumStringTypeHandler<MovementType>());
        SqlMapper.AddTypeHandler(new EnumStringTypeHandler<PaymentMethod>());
        SqlMapper.AddTypeHandler(new EnumStringTypeHandler<SaleStatus>());
        SqlMapper.AddTypeHandler(new EnumStringTypeHandler<PurchaseOrderStatus>());

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .Build();

        var connectionString = configuration["Database:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            MessageBox.Show(
                "No database connection string configured. Create appsettings.Local.json with your connection details.",
                "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        var services = new ServiceCollection();

        services.AddSingleton(new DbConnectionFactory(connectionString));
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();

        services.AddSingleton<ICurrentSession, CurrentSession>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<LoginViewModel>();

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<PlaceholderViewModel>();
        services.AddTransient<MainWindow>();

        Services = services.BuildServiceProvider();

        var loginWindow = Services.GetRequiredService<LoginWindow>();
        var result = loginWindow.ShowDialog();

        if (result != true)
        {
            Shutdown();
            return;
        }

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
