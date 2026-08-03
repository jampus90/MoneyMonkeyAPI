namespace MoneyMonkey.Data.Entities;

public class CreditCardInstallment
{
    public int CreditCardInstallmentId { get; set; }
    public int CreditCardPurchaseId { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal Value { get; set; }
    public int InvoiceMonth { get; set; }
    public int InvoiceYear { get; set; }
}
