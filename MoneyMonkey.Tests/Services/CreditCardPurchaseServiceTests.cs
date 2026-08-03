using MoneyMonkey.Application.Services;
using MoneyMonkey.Communication.Enums;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Data.Repository;
using MoneyMonkey.Tests.TestHelpers;

namespace MoneyMonkey.Tests.Services;

public class CreditCardPurchaseServiceTests
{
    private static async Task<(CreditCardPurchaseService Service, int CreditCardId)> CreateSutWithCard()
    {
        var context = DbContextFactory.CreateInMemoryContext();
        var creditCardRepository = new CreditCardRepository(context);
        var purchaseRepository = new CreditCardPurchaseRepository(context);
        var service = new CreditCardPurchaseService(purchaseRepository);

        var card = await creditCardRepository.CreateCreditCard(1, new CreditCardRequest
        {
            Name = "Nubank Roxinho",
            Brand = CardBrand.Mastercard,
            LastFourDigits = "1234",
            ClosingDay = 10,
            DueDay = 17
        });

        return (service, card.CreditCardId);
    }

    [Fact]
    public async Task CreatePurchase_WithValidCard_ReturnsCreatedInstallment()
    {
        // Arrange
        var (service, creditCardId) = await CreateSutWithCard();
        var request = new CreditCardPurchaseRequest
        {
            Description = "Supermercado",
            TotalValue = 100m,
            PurchaseDate = new DateOnly(2026, 8, 5)
        };

        // Act
        var result = await service.CreatePurchase(1, creditCardId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Supermercado", result!.Description);
        Assert.Equal(1, result.InstallmentsCount);
    }

    [Fact]
    public async Task CreatePurchase_WithCreditCardFromAnotherUser_ReturnsNull()
    {
        // Arrange
        var (service, creditCardId) = await CreateSutWithCard();
        var request = new CreditCardPurchaseRequest
        {
            Description = "Suspicious",
            TotalValue = 50m
        };

        // Act
        var result = await service.CreatePurchase(2, creditCardId, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetInvoice_ReturnsInstallmentsAndTotalForTheGivenCycle()
    {
        // Arrange
        var (service, creditCardId) = await CreateSutWithCard();
        await service.CreatePurchase(1, creditCardId, new CreditCardPurchaseRequest
        {
            Description = "Supermercado",
            TotalValue = 100m,
            PurchaseDate = new DateOnly(2026, 8, 5)
        });

        // Act
        var invoice = await service.GetInvoice(1, creditCardId, 8, 2026);

        // Assert
        Assert.NotNull(invoice);
        Assert.Equal(100m, invoice!.TotalValue);
        Assert.Single(invoice.Installments);
    }
}
