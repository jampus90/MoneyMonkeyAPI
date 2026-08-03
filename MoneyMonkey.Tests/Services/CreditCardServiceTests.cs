using MoneyMonkey.Application.Services;
using MoneyMonkey.Communication.Enums;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Data.Repository;
using MoneyMonkey.Tests.TestHelpers;

namespace MoneyMonkey.Tests.Services;

public class CreditCardServiceTests
{
    private static CreditCardService CreateSut()
    {
        var context = DbContextFactory.CreateInMemoryContext();
        var repository = new CreditCardRepository(context);

        return new CreditCardService(repository);
    }

    [Fact]
    public async Task CreateCreditCard_WithValidRequest_ReturnsCreatedCreditCard()
    {
        // Arrange
        var service = CreateSut();
        var request = new CreditCardRequest
        {
            Name = "Nubank Roxinho",
            Brand = CardBrand.Mastercard,
            LastFourDigits = "1234",
            ClosingDay = 10,
            DueDay = 17
        };

        // Act
        var result = await service.CreateCreditCard(1, request);

        // Assert
        Assert.Equal("Nubank Roxinho", result.Name);
        Assert.Equal("1234", result.LastFourDigits);
    }

    [Fact]
    public async Task GetAllCreditCards_ReturnsOnlyCardsForRequestedUser()
    {
        // Arrange
        var service = CreateSut();
        await service.CreateCreditCard(1, new CreditCardRequest
        {
            Name = "Mine",
            Brand = CardBrand.Visa,
            LastFourDigits = "1111",
            ClosingDay = 5,
            DueDay = 12
        });
        await service.CreateCreditCard(2, new CreditCardRequest
        {
            Name = "Not mine",
            Brand = CardBrand.Elo,
            LastFourDigits = "2222",
            ClosingDay = 5,
            DueDay = 12
        });

        // Act
        var result = await service.GetAllCreditCards(1);

        // Assert
        var card = Assert.Single(result.CreditCardResponses!);
        Assert.Equal("Mine", card.Name);
    }
}
