using Dapper;
using InventoryPOS.Core.Interfaces;
using InventoryPOS.Core.Models;

namespace InventoryPOS.Data.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public PurchaseOrderRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PurchaseOrder> CreateAsync(int supplierId, int createdByUserId, List<PurchaseOrderItemInput> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("A purchase order must have at least one item.", nameof(items));

        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var orderId = await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO purchase_orders (supplier_id, status, created_by)
                  VALUES (@SupplierId, @Status, @CreatedBy);
                  SELECT LAST_INSERT_ID();",
                new { SupplierId = supplierId, Status = PurchaseOrderStatus.Pending, CreatedBy = createdByUserId },
                transaction);

            foreach (var item in items)
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO purchase_order_items
                        (purchase_order_id, product_id, quantity_ordered, quantity_received, unit_cost)
                      VALUES (@OrderId, @ProductId, @QuantityOrdered, 0, @UnitCost)",
                    new
                    {
                        OrderId = orderId,
                        item.ProductId,
                        item.QuantityOrdered,
                        item.UnitCost
                    },
                    transaction);
            }

            transaction.Commit();

            return new PurchaseOrder
            {
                Id = orderId,
                SupplierId = supplierId,
                Status = PurchaseOrderStatus.Pending,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<PurchaseOrder?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<PurchaseOrder>(
            @"SELECT id, supplier_id AS SupplierId, status,
                     created_by AS CreatedBy, created_at AS CreatedAt
              FROM purchase_orders WHERE id = @Id",
            new { Id = id });
    }

    public async Task<IEnumerable<PurchaseOrderItem>> GetItemsByOrderIdAsync(int purchaseOrderId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<PurchaseOrderItem>(
            @"SELECT id, purchase_order_id AS PurchaseOrderId, product_id AS ProductId,
                     quantity_ordered AS QuantityOrdered, quantity_received AS QuantityReceived,
                     unit_cost AS UnitCost
              FROM purchase_order_items WHERE purchase_order_id = @PurchaseOrderId",
            new { PurchaseOrderId = purchaseOrderId });
    }

    public async Task ReceiveAsync(int purchaseOrderId, int receivedByUserId)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var order = await connection.QuerySingleOrDefaultAsync<PurchaseOrder>(
                "SELECT id, status FROM purchase_orders WHERE id = @Id FOR UPDATE",
                new { Id = purchaseOrderId }, transaction);

            if (order == null)
                throw new InvalidOperationException($"Purchase order {purchaseOrderId} not found.");

            if (order.Status != PurchaseOrderStatus.Pending)
                throw new InvalidOperationException(
                    $"Purchase order {purchaseOrderId} is already '{order.Status}', cannot receive.");

            var items = await connection.QueryAsync<PurchaseOrderItem>(
                @"SELECT id, product_id AS ProductId, quantity_ordered AS QuantityOrdered,
                         quantity_received AS QuantityReceived, unit_cost AS UnitCost
                  FROM purchase_order_items WHERE purchase_order_id = @Id",
                new { Id = purchaseOrderId }, transaction);

            foreach (var item in items)
            {
                await connection.ExecuteAsync(
                    "UPDATE purchase_order_items SET quantity_received = quantity_ordered WHERE id = @Id",
                    new { item.Id }, transaction);

                await connection.ExecuteAsync(
                    @"INSERT INTO stock_movements (product_id, movement_type, quantity_change, note, performed_by)
                      VALUES (@ProductId, @MovementType, @QuantityChange, @Note, @PerformedBy)",
                    new
                    {
                        item.ProductId,
                        MovementType = MovementType.Purchase,
                        QuantityChange = item.QuantityOrdered,
                        Note = $"Received PO #{purchaseOrderId}",
                        PerformedBy = receivedByUserId
                    },
                    transaction);

                await connection.ExecuteAsync(
                    "UPDATE stock_levels SET quantity_on_hand = quantity_on_hand + @Quantity WHERE product_id = @ProductId",
                    new { Quantity = item.QuantityOrdered, item.ProductId },
                    transaction);
            }

            await connection.ExecuteAsync(
                "UPDATE purchase_orders SET status = @Status WHERE id = @Id",
                new { Status = PurchaseOrderStatus.Received, Id = purchaseOrderId },
                transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
