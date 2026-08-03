using MoneyMonkey.Communication.Enums;

namespace MoneyMonkey.Communication.Response;
public class CreditCardResponse
{
    public int CreditCardId { get; set; }
    public string? Name { get; set; }
    public CardBrand Brand { get; set; }
    public string? LastFourDigits { get; set; }
    public int ClosingDay { get; set; }
    public int DueDay { get; set; }
    public decimal? CreditLimit { get; set; }
}
