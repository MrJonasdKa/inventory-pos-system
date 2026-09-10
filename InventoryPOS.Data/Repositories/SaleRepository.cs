using Dapper;
using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;

namespace InventoryPOS.Data.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public SaleRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Sale> CompleteSaleAsync(
        int cashierId, PaymentMethod paymentMethod, decimal discountAmount, List<SaleItemInput> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("A sale must have at least one item.", nameof(items));

        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            decimal totalAmount = 0;
            var saleItemsToInsert = new List<(int ProductId, int Quantity, decimal UnitPrice, decimal LineTotal)>();

            // Look up current price + stock for every product first,
            // and fail the whole sale before writing anything if stock is insufficient.
            foreach (var item in items)
            {
                var product = await connection.QuerySingleOrDefaultAsync<Product>(
                    "SELECT id, unit_price AS UnitPrice FROM products WHERE id = @Id AND is_active = 1",
                    new { Id = item.ProductId }, transaction);

                if (product == null)
                    throw new InvalidOperationException($"Product {item.ProductId} not found or inactive.");

                var stock = await connection.QuerySingleOrDefaultAsync<int>(
                    "SELECT quantity_on_hand FROM stock_levels WHERE product_id = @Id FOR UPDATE",
                    new { Id = item.ProductId }, transaction);

                if (stock < item.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for product {item.ProductId}: has {stock}, needs {item.Quantity}.");

                var lineTotal = product.UnitPrice * item.Quantity;
                totalAmount += lineTotal;
                saleItemsToInsert.Add((item.ProductId, item.Quantity, product.UnitPrice, lineTotal));
            }

            totalAmount -= discountAmount;

            var saleNumber = $"SALE-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}";

            var saleId = await connection.ExecuteScalarAsync<long>(
                @"INSERT INTO sales (sale_number, cashier_id, total_amount, discount_amount, payment_method, status)
                  VALUES (@SaleNumber, @CashierId, @TotalAmount, @DiscountAmount, @PaymentMethod, @Status);
                  SELECT LAST_INSERT_ID();",
                new
                {
                    SaleNumber = saleNumber,
                    CashierId = cashierId,
                    TotalAmount = totalAmount,
                    DiscountAmount = discountAmount,
                    PaymentMethod = paymentMethod,
                    Status = SaleStatus.Completed
                },
                transaction);

            foreach (var line in saleItemsToInsert)
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO sale_items (sale_id, product_id, quantity, unit_price_at_sale, line_total)
                      VALUES (@SaleId, @ProductId, @Quantity, @UnitPrice, @LineTotal)",
                    new
                    {
                        SaleId = saleId,
                        line.ProductId,
                        line.Quantity,
                        UnitPrice = line.UnitPrice,
                        line.LineTotal
                    },
                    transaction);

                await connection.ExecuteAsync(
                    @"INSERT INTO stock_movements (product_id, movement_type, quantity_change, note, performed_by)
                      VALUES (@ProductId, @MovementType, @QuantityChange, @Note, @PerformedBy)",
                    new
                    {
                        line.ProductId,
                        MovementType = MovementType.Sale,
                        QuantityChange = -line.Quantity,
                        Note = $"Sale {saleNumber}",
                        PerformedBy = cashierId
                    },
                    transaction);

                await connection.ExecuteAsync(
                    "UPDATE stock_levels SET quantity_on_hand = quantity_on_hand - @Quantity WHERE product_id = @ProductId",
                    new { line.Quantity, line.ProductId },
                    transaction);
            }

            transaction.Commit();

            return new Sale
            {
                Id = saleId,
                SaleNumber = saleNumber,
                CashierId = cashierId,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                PaymentMethod = paymentMethod,
                Status = SaleStatus.Completed,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<Sale?> GetByIdAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Sale>(
            @"SELECT id, sale_number AS SaleNumber, cashier_id AS CashierId,
                     total_amount AS TotalAmount, discount_amount AS DiscountAmount,
                     payment_method AS PaymentMethod, status, created_at AS CreatedAt
              FROM sales WHERE id = @Id",
            new { Id = id });
    }

    public async Task<IEnumerable<SaleItem>> GetItemsBySaleIdAsync(long saleId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<SaleItem>(
            @"SELECT id, sale_id AS SaleId, product_id AS ProductId, quantity,
                     unit_price_at_sale AS UnitPriceAtSale, line_total AS LineTotal
              FROM sale_items WHERE sale_id = @SaleId",
            new { SaleId = saleId });
    }
}
