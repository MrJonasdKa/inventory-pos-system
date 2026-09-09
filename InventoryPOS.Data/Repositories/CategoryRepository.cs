using Dapper;
using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;

namespace InventoryPOS.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public CategoryRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Category>(
            "SELECT id, name, description FROM categories ORDER BY name");
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Category>(
            "SELECT id, name, description FROM categories WHERE id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(Category category)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "INSERT INTO categories (name, description) VALUES (@Name, @Description); " +
                  "SELECT LAST_INSERT_ID();";
        return await connection.ExecuteScalarAsync<int>(sql, category);
    }

    public async Task UpdateAsync(Category category)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE categories SET name = @Name, description = @Description WHERE id = @Id",
            category);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM categories WHERE id = @Id", new { Id = id });
    }
}
