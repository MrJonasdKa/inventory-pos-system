using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryPOS.Core.Interfaces;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace InventoryPOS.Client.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly IReportRepository _reportRepository;

    [ObservableProperty]
    private ISeries[] salesSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private string[] salesLabels = Array.Empty<string>();

    [ObservableProperty]
    private ISeries[] topProductsSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private string[] topProductsLabels = Array.Empty<string>();

    [ObservableProperty]
    private decimal totalStockValue;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public ReportsViewModel(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        ErrorMessage = string.Empty;
        try
        {
            var salesByDay = (await _reportRepository.GetSalesByDayAsync(30)).ToList();
            SalesLabels = salesByDay.Select(s => s.Date.ToString("MM/dd")).ToArray();
            SalesSeries = new ISeries[]
            {
                new LineSeries<decimal>
                {
                    Values = salesByDay.Select(s => s.Total).ToArray(),
                    Name = "Sales"
                }
            };

            var topProducts = (await _reportRepository.GetTopProductsByRevenueAsync(5)).ToList();
            TopProductsLabels = topProducts.Select(p => p.Name).ToArray();
            TopProductsSeries = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Values = topProducts.Select(p => p.Revenue).ToArray(),
                    Name = "Revenue"
                }
            };

            TotalStockValue = await _reportRepository.GetTotalStockValueAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load reports: {ex.Message}";
        }
    }
}
