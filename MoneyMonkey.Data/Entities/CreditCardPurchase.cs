namespace MoneyMonkey.Data.Entities;

public class CreditCardPurchase
{
    public int CreditCardPurchaseId { get; set; }
    public long UserId { get; set; }
    public int CreditCardId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public int InstallmentsCount { get; set; }
    public int? CategoryId { get; set; }
    public bool IsSubscription { get; set; }
    public DateTime CreatedAt { get; set; }
}
