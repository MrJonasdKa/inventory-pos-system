namespace InventoryPOS.Core.Models;

public enum PaymentMethod
{
    Cash,
    Card,
    Other
}

public enum SaleStatus
{
    Completed,
    Voided
}

public class Sale
{
    public long Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public int CashierId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Completed;
    public DateTime CreatedAt { get; set; }
}
