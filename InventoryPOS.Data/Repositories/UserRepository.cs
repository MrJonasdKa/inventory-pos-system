using Dapper;
using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;

namespace InventoryPOS.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public UserRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private const string SelectColumns =
        @"id, username, password_hash AS PasswordHash, full_name AS FullName,
          role_id AS RoleId, is_active AS IsActive,
          created_at AS CreatedAt, updated_at AS UpdatedAt";

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<User>(
            $"SELECT {SelectColumns} FROM users WHERE is_active = 1 ORDER BY full_name");
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(
            $"SELECT {SelectColumns} FROM users WHERE id = @Id", new { Id = id });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(
            $"SELECT {SelectColumns} FROM users WHERE username = @Username",
            new { Username = username });
    }

    public async Task<int> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO users (username, password_hash, full_name, role_id, is_active)
                    VALUES (@Username, @PasswordHash, @FullName, @RoleId, @IsActive);
                    SELECT LAST_INSERT_ID();";
        return await connection.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task UpdateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            @"UPDATE users SET full_name = @FullName, role_id = @RoleId
              WHERE id = @Id",
            user);
    }

    public async Task DeactivateAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("UPDATE users SET is_active = 0 WHERE id = @Id", new { Id = id });
    }
}
