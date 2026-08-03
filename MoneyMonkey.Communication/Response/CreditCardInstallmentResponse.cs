namespace MoneyMonkey.Communication.Response;
public class CreditCardInstallmentResponse
{
    public int CreditCardInstallmentId { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public bool IsSubscription { get; set; }
    public int InstallmentNumber { get; set; }
    public int InstallmentsCount { get; set; }
    public decimal Value { get; set; }
    public DateOnly PurchaseDate { get; set; }
}
