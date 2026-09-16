using Dapper;
using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;

namespace InventoryPOS.Data.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public ReportRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<SalesByDay>> GetSalesByDayAsync(int days)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<SalesByDay>(
            @"SELECT DATE(created_at) AS Date, SUM(total_amount) AS Total
              FROM sales
              WHERE status = 'completed' AND created_at >= DATE_SUB(CURDATE(), INTERVAL @Days DAY)
              GROUP BY DATE(created_at)
              ORDER BY DATE(created_at)",
            new { Days = days });
    }

    public async Task<IEnumerable<ProductRevenue>> GetTopProductsByRevenueAsync(int topN)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ProductRevenue>(
            @"SELECT p.name AS Name, SUM(si.line_total) AS Revenue
              FROM sale_items si
              JOIN products p ON p.id = si.product_id
              JOIN sales s ON s.id = si.sale_id
              WHERE s.status = 'completed'
              GROUP BY p.id, p.name
              ORDER BY Revenue DESC
              LIMIT @TopN",
            new { TopN = topN });
    }

    public async Task<decimal> GetTotalStockValueAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<decimal>(
            @"SELECT COALESCE(SUM(p.cost_price * sl.quantity_on_hand), 0)
              FROM products p
              JOIN stock_levels sl ON sl.product_id = p.id
              WHERE p.is_active = 1");
    }
}
