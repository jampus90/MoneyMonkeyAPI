using Microsoft.EntityFrameworkCore;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Communication.Response;
using MoneyMonkey.Data.Entities;

namespace MoneyMonkey.Data.Repository;
public class CreditCardRepository
{
    private readonly MoneyMonkeyDbContext _context;

    public CreditCardRepository(MoneyMonkeyDbContext context)
    {
        _context = context;
    }

    public async Task<CreditCardResponseList> GetAllCreditCards(long userId)
    {
        var creditCards = await _context.CreditCards
            .Where(c => c.UserId == userId)
            .Select(c => new CreditCardResponse
            {
                CreditCardId = c.CreditCardId,
                Name = c.Name,
                Brand = c.Brand,
                LastFourDigits = c.LastFourDigits,
                ClosingDay = c.ClosingDay,
                DueDay = c.DueDay,
                CreditLimit = c.CreditLimit
            })
            .ToListAsync();

        return new CreditCardResponseList { CreditCardResponses = creditCards };
    }

    public async Task<CreditCardResponse> CreateCreditCard(long userId, CreditCardRequest request)
    {
        var creditCard = new CreditCard
        {
            UserId = userId,
            Name = request.Name,
            Brand = request.Brand,
            LastFourDigits = request.LastFourDigits,
            ClosingDay = request.ClosingDay,
            DueDay = request.DueDay,
            CreditLimit = request.CreditLimit,
            CreatedAt = DateTime.UtcNow
        };

        _context.CreditCards.Add(creditCard);
        await _context.SaveChangesAsync();

        return new CreditCardResponse
        {
            CreditCardId = creditCard.CreditCardId,
            Name = creditCard.Name,
            Brand = creditCard.Brand,
            LastFourDigits = creditCard.LastFourDigits,
            ClosingDay = creditCard.ClosingDay,
            DueDay = creditCard.DueDay,
            CreditLimit = creditCard.CreditLimit
        };
    }
}
