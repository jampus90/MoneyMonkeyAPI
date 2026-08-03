using System.ComponentModel.DataAnnotations;

namespace MoneyMonkey.Communication.Request;
public class CreditCardPurchaseRequest
{
    [StringLength(100)]
    public required string Description { get; set; }
    [Range(0.01, double.MaxValue)]
    public required decimal TotalValue { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    [Range(1, 48)]
    public int? InstallmentsCount { get; set; }
    public int? CategoryId { get; set; }
    public bool IsSubscription { get; set; }
}
