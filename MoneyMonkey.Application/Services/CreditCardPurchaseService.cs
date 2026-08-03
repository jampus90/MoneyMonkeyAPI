using MoneyMonkey.Communication.Request;
using MoneyMonkey.Communication.Response;
using MoneyMonkey.Data.Repository;

namespace MoneyMonkey.Application.Services;
public class CreditCardPurchaseService
{
    private readonly CreditCardPurchaseRepository _creditCardPurchaseRepository;

    public CreditCardPurchaseService(CreditCardPurchaseRepository creditCardPurchaseRepository)
    {
        _creditCardPurchaseRepository = creditCardPurchaseRepository;
    }

    public async Task<CreditCardInstallmentResponse?> CreatePurchase(long userId, int creditCardId, CreditCardPurchaseRequest request)
    {
        return await _creditCardPurchaseRepository.CreatePurchase(userId, creditCardId, request);
    }

    public async Task<CreditCardInvoiceResponse?> GetInvoice(long userId, int creditCardId, int? month, int? year)
    {
        return await _creditCardPurchaseRepository.GetInvoice(userId, creditCardId, month, year);
    }
}
