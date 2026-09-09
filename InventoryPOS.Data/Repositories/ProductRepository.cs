using System.Data;
using Dapper;
using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;

namespace InventoryPOS.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public ProductRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Product>(
            @"SELECT id, sku, name, description, category_id AS CategoryId,
                     unit_price AS UnitPrice, cost_price AS CostPrice,
                     reorder_threshold AS ReorderThreshold, is_active AS IsActive,
                     created_at AS CreatedAt, updated_at AS UpdatedAt
              FROM products
              WHERE is_active = 1
              ORDER BY name");
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Product>(
            @"SELECT id, sku, name, description, category_id AS CategoryId,
                     unit_price AS UnitPrice, cost_price AS CostPrice,
                     reorder_threshold AS ReorderThreshold, is_active AS IsActive,
                     created_at AS CreatedAt, updated_at AS UpdatedAt
              FROM products WHERE id = @Id",
            new { Id = id });
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Product>(
            @"SELECT id, sku, name, description, category_id AS CategoryId,
                     unit_price AS UnitPrice, cost_price AS CostPrice,
                     reorder_threshold AS ReorderThreshold, is_active AS IsActive,
                     created_at AS CreatedAt, updated_at AS UpdatedAt
              FROM products WHERE sku = @Sku",
            new { Sku = sku });
    }

    public async Task<int> CreateAsync(Product product)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var insertSql = @"INSERT INTO products
                    (sku, name, description, category_id, unit_price, cost_price, reorder_threshold, is_active)
                VALUES
                    (@Sku, @Name, @Description, @CategoryId, @UnitPrice, @CostPrice, @ReorderThreshold, @IsActive);
                SELECT LAST_INSERT_ID();";

            var newId = await connection.ExecuteScalarAsync<int>(insertSql, product, transaction);

            // Every product needs a starting stock_levels row — created here
            // as part of the same transaction, so we never end up with a
            // product that has no stock record at all.
            await connection.ExecuteAsync(
                "INSERT INTO stock_levels (product_id, quantity_on_hand) VALUES (@ProductId, 0)",
                new { ProductId = newId },
                transaction);

            transaction.Commit();
            return newId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task UpdateAsync(Product product)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            @"UPDATE products SET
                    name = @Name, description = @Description, category_id = @CategoryId,
                    unit_price = @UnitPrice, cost_price = @CostPrice,
                    reorder_threshold = @ReorderThreshold
              WHERE id = @Id",
            product);
    }

    public async Task DeactivateAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE products SET is_active = 0 WHERE id = @Id",
            new { Id = id });
    }
}
