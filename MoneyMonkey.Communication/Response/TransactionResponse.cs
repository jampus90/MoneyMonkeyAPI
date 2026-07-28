using MoneyMonkey.Communication.Enums;

namespace MoneyMonkey.Communication.Response;
public class TransactionResponse
{
    public int TransactionId { get; set; }
    public string? TransactionName { get; set; }
    public decimal Value { get; set; }
    public TransactionType Type { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public int? CategoryId { get; set; }
    public DateOnly TransactionDate { get; set; }
}
