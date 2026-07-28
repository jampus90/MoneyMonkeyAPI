using System.ComponentModel.DataAnnotations;
using MoneyMonkey.Communication.Enums;

namespace MoneyMonkey.Communication.Request;
public class TransactionRequest
{
    [StringLength(100)]
    public required string TransactionName { get; set; }
    [Range(0.01, double.MaxValue)]
    public required decimal Value { get; set; }
    public required TransactionType Type { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public int? CategoryId { get; set; }
    public DateOnly? TransactionDate { get; set; }
}
