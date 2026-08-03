using MoneyMonkey.Communication.Enums;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Data.Repository;
using MoneyMonkey.Tests.TestHelpers;

namespace MoneyMonkey.Tests.Repository;

public class CreditCardPurchaseRepositoryTests
{
    private static async Task<(CreditCardPurchaseRepository PurchaseRepository, int CreditCardId)> CreateSutWithCard(
        long userId = 1, int closingDay = 10, int dueDay = 17)
    {
        var context = DbContextFactory.CreateInMemoryContext();
        var creditCardRepository = new CreditCardRepository(context);
        var purchaseRepository = new CreditCardPurchaseRepository(context);

        var card = await creditCardRepository.CreateCreditCard(userId, new CreditCardRequest
        {
            Name = "Nubank Roxinho",
            Brand = CardBrand.Mastercard,
            LastFourDigits = "1234",
            ClosingDay = closingDay,
            DueDay = dueDay
        });

        return (purchaseRepository, card.CreditCardId);
    }

    [Fact]
    public async Task CreatePurchase_BeforeClosingDay_FallsIntoCurrentMonthInvoice()
    {
        // Arrange
        var (repository, creditCardId) = await CreateSutWithCard(closingDay: 10);
        var request = new CreditCardPurchaseRequest
        {
            Description = "Supermercado",
            TotalValue = 100m,
            PurchaseDate = new DateOnly(2026, 8, 5)
        };

        // Act
        var result = await repository.CreatePurchase(1, creditCardId, request);
        var invoice = await repository.GetInvoice(1, creditCardId, 8, 2026);

        // Assert
        Assert.NotNull(result);
        var installment = Assert.Single(invoice!.Installments);
        Assert.Equal(100m, installment.Value);
        Assert.Equal(new DateOnly(2026, 8, 17), invoice.DueDate);
    }

    [Fact]
    public async Task CreatePurchase_AfterClosingDay_FallsIntoNextMonthInvoice()
    {
        // Arrange
        var (repository, creditCardId) = await CreateSutWithCard(closingDay: 10);
        var request = new CreditCardPurchaseRequest
        {
            Description = "Supermercado",
            TotalValue = 100m,
            PurchaseDate = new DateOnly(2026, 8, 15)
        };

        // Act
        await repository.CreatePurchase(1, creditCardId, request);
        var augustInvoice = await repository.GetInvoice(1, creditCardId, 8, 2026);
        var septemberInvoice = await repository.GetInvoice(1, creditCardId, 9, 2026);

        // Assert
        Assert.Empty(augustInvoice!.Installments);
        Assert.Single(septemberInvoice!.Installments);
    }

    [Fact]
    public async Task CreatePurchase_WithInstallments_SplitsAcrossConsecutiveInvoicesSummingToTotal()
    {
        // Arrange
        var (repository, creditCardId) = await CreateSutWithCard(closingDay: 10);
        var request = new CreditCardPurchaseRequest
        {
            Description = "Notebook",
            TotalValue = 100m,
            PurchaseDate = new DateOnly(2026, 8, 5),
            InstallmentsCount = 3
        };

        // Act
        await repository.CreatePurchase(1, creditCardId, request);
        var august = await repository.GetInvoice(1, creditCardId, 8, 2026);
        var september = await repository.GetInvoice(1, creditCardId, 9, 2026);
        var october = await repository.GetInvoice(1, creditCardId, 10, 2026);

        // Assert
        var total = august!.Installments[0].Value + september!.Installments[0].Value + october!.Installments[0].Value;
        Assert.Equal(100m, total);
        Assert.Equal(1, august.Installments[0].InstallmentNumber);
        Assert.Equal(3, october.Installments[0].InstallmentNumber);
    }

    [Fact]
    public async Task CreatePurchase_WithCreditCardFromAnotherUser_ReturnsNull()
    {
        // Arrange
        var (repository, creditCardId) = await CreateSutWithCard(userId: 1);
        var request = new CreditCardPurchaseRequest
        {
            Description = "Suspicious",
            TotalValue = 50m
        };

        // Act
        var result = await repository.CreatePurchase(2, creditCardId, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetInvoice_WithoutMonthOrYear_DefaultsToCurrentMonth()
    {
        // Arrange
        var (repository, creditCardId) = await CreateSutWithCard(closingDay: 28);
        var now = DateTime.UtcNow;
        var purchaseDate = new DateOnly(now.Year, now.Month, 1);
        await repository.CreatePurchase(1, creditCardId, new CreditCardPurchaseRequest
        {
            Description = "Assinatura",
            TotalValue = 39.90m,
            PurchaseDate = purchaseDate
        });

        // Act
        var invoice = await repository.GetInvoice(1, creditCardId, month: null, year: null);

        // Assert
        Assert.NotNull(invoice);
        Assert.Equal(now.Month, invoice!.InvoiceMonth);
        Assert.Equal(now.Year, invoice.InvoiceYear);
        Assert.Single(invoice.Installments);
    }

    [Fact]
    public async Task CreatePurchase_MarkedAsSubscription_IsReflectedInInvoice()
    {
        // Arrange
        var (repository, creditCardId) = await CreateSutWithCard(closingDay: 10);
        var request = new CreditCardPurchaseRequest
        {
            Description = "Netflix",
            TotalValue = 39.90m,
            PurchaseDate = new DateOnly(2026, 8, 5),
            IsSubscription = true
        };

        // Act
        await repository.CreatePurchase(1, creditCardId, request);
        var invoice = await repository.GetInvoice(1, creditCardId, 8, 2026);

        // Assert
        Assert.True(invoice!.Installments[0].IsSubscription);
    }
}
