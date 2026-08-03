using MoneyMonkey.Communication.Enums;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Data.Repository;
using MoneyMonkey.Tests.TestHelpers;

namespace MoneyMonkey.Tests.Repository;

public class CreditCardRepositoryTests
{
    [Fact]
    public async Task CreateCreditCard_PersistsCreditCardWithoutFullCardNumber()
    {
        // Arrange
        var context = DbContextFactory.CreateInMemoryContext();
        var repository = new CreditCardRepository(context);
        const long userId = 1;
        var request = new CreditCardRequest
        {
            Name = "Nubank Roxinho",
            Brand = CardBrand.Mastercard,
            LastFourDigits = "1234",
            ClosingDay = 10,
            DueDay = 17,
            CreditLimit = 5000m
        };

        // Act
        var result = await repository.CreateCreditCard(userId, request);

        // Assert
        Assert.Equal("Nubank Roxinho", result.Name);
        Assert.Equal("1234", result.LastFourDigits);
        Assert.Equal(CardBrand.Mastercard, result.Brand);
    }

    [Fact]
    public async Task GetAllCreditCards_ReturnsOnlyRequestedUserCards()
    {
        // Arrange
        var context = DbContextFactory.CreateInMemoryContext();
        var repository = new CreditCardRepository(context);
        await repository.CreateCreditCard(1, new CreditCardRequest
        {
            Name = "Mine",
            Brand = CardBrand.Visa,
            LastFourDigits = "1111",
            ClosingDay = 5,
            DueDay = 12
        });
        await repository.CreateCreditCard(2, new CreditCardRequest
        {
            Name = "Not mine",
            Brand = CardBrand.Elo,
            LastFourDigits = "2222",
            ClosingDay = 5,
            DueDay = 12
        });

        // Act
        var result = await repository.GetAllCreditCards(1);

        // Assert
        var card = Assert.Single(result.CreditCardResponses!);
        Assert.Equal("Mine", card.Name);
    }
}
