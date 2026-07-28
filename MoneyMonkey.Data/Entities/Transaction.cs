using MoneyMonkey.Communication.Enums;

namespace MoneyMonkey.Data.Entities;

public class Transaction
{
    public int TransactionId { get; set; }
    public long UserId { get; set; }
    public string TransactionName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public TransactionType Type { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public int? CategoryId { get; set; }
    public DateOnly TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
