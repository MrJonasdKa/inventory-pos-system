using Dapper;
using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;

namespace InventoryPOS.Data.Repositories;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public StockMovementRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<StockMovement>(
            @"SELECT id, product_id AS ProductId, movement_type AS MovementType,
                     quantity_change AS QuantityChange, note, performed_by AS PerformedBy,
                     created_at AS CreatedAt
              FROM stock_movements
              WHERE product_id = @ProductId
              ORDER BY created_at DESC",
            new { ProductId = productId });
    }

    public async Task<long> CreateAsync(StockMovement movement)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO stock_movements
                    (product_id, movement_type, quantity_change, note, performed_by)
                VALUES
                    (@ProductId, @MovementType, @QuantityChange, @Note, @PerformedBy);
                SELECT LAST_INSERT_ID();";
        return await connection.ExecuteScalarAsync<long>(sql, movement);
    }
}
