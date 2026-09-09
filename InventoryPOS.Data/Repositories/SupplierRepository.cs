using Dapper;
using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;

namespace InventoryPOS.Data.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public SupplierRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private const string SelectColumns =
        @"id, name, contact_name AS ContactName, phone, email, address, created_at AS CreatedAt";

    public async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Supplier>(
            $"SELECT {SelectColumns} FROM suppliers ORDER BY name");
    }

    public async Task<Supplier?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Supplier>(
            $"SELECT {SelectColumns} FROM suppliers WHERE id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(Supplier supplier)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO suppliers (name, contact_name, phone, email, address)
                    VALUES (@Name, @ContactName, @Phone, @Email, @Address);
                    SELECT LAST_INSERT_ID();";
        return await connection.ExecuteScalarAsync<int>(sql, supplier);
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            @"UPDATE suppliers SET name = @Name, contact_name = @ContactName,
                    phone = @Phone, email = @Email, address = @Address
              WHERE id = @Id",
            supplier);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM suppliers WHERE id = @Id", new { Id = id });
    }
}
