using System.ComponentModel.DataAnnotations;
using MoneyMonkey.Communication.Enums;

namespace MoneyMonkey.Communication.Request;
public class CreditCardRequest
{
    [StringLength(50)]
    public required string Name { get; set; }
    public required CardBrand Brand { get; set; }
    [StringLength(4, MinimumLength = 4)]
    public required string LastFourDigits { get; set; }
    [Range(1, 28)]
    public required int ClosingDay { get; set; }
    [Range(1, 28)]
    public required int DueDay { get; set; }
    [Range(0.01, double.MaxValue)]
    public decimal? CreditLimit { get; set; }
}
