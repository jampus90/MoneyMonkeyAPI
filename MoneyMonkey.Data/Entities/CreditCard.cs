using MoneyMonkey.Communication.Enums;

namespace MoneyMonkey.Data.Entities;

public class CreditCard
{
    public int CreditCardId { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CardBrand Brand { get; set; }
    public string LastFourDigits { get; set; } = string.Empty;
    public int ClosingDay { get; set; }
    public int DueDay { get; set; }
    public decimal? CreditLimit { get; set; }
    public DateTime CreatedAt { get; set; }
}
