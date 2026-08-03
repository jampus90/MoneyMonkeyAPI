namespace MoneyMonkey.Communication.Response;
public class CreditCardInvoiceResponse
{
    public int CreditCardId { get; set; }
    public int InvoiceMonth { get; set; }
    public int InvoiceYear { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal TotalValue { get; set; }
    public List<CreditCardInstallmentResponse> Installments { get; set; } = new();
}
