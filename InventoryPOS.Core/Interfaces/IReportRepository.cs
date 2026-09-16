using InventoryPOS.Core.Models;

namespace InventoryPOS.Core.Interfaces;

public interface IReportRepository
{
    Task<IEnumerable<SalesByDay>> GetSalesByDayAsync(int days);
    Task<IEnumerable<ProductRevenue>> GetTopProductsByRevenueAsync(int topN);
    Task<decimal> GetTotalStockValueAsync();
}
