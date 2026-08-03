using MoneyMonkey.Communication.Request;
using MoneyMonkey.Communication.Response;
using MoneyMonkey.Data.Repository;

namespace MoneyMonkey.Application.Services;
public class CreditCardService
{
    private readonly CreditCardRepository _creditCardRepository;

    public CreditCardService(CreditCardRepository creditCardRepository)
    {
        _creditCardRepository = creditCardRepository;
    }

    public async Task<CreditCardResponseList> GetAllCreditCards(long userId)
    {
        return await _creditCardRepository.GetAllCreditCards(userId);
    }

    public async Task<CreditCardResponse> CreateCreditCard(long userId, CreditCardRequest request)
    {
        return await _creditCardRepository.CreateCreditCard(userId, request);
    }
}
