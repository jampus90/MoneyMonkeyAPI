using MoneyMonkey.Application.Services;
using MoneyMonkey.Communication.Enums;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Data.Repository;
using MoneyMonkey.Tests.TestHelpers;

namespace MoneyMonkey.Tests.Services;

public class TransactionServiceTests
{
    private static (TransactionService Service, CategoryRepository CategoryRepository) CreateSut()
    {
        var context = DbContextFactory.CreateInMemoryContext();
        var transactionRepository = new TransactionRepository(context);
        var categoryRepository = new CategoryRepository(context);
        var service = new TransactionService(transactionRepository);

        return (service, categoryRepository);
    }

    [Fact]
    public async Task CreateTransaction_WithValidRequest_ReturnsCreatedTransaction()
    {
        // Arrange
        var (service, _) = CreateSut();
        const long userId = 1;
        var request = new TransactionRequest
        {
            TransactionName = "Freelance payment",
            Value = 150.50m,
            Type = TransactionType.Entrada,
            PaymentMethod = PaymentMethod.Pix
        };

        // Act
        var result = await service.CreateTransaction(userId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Freelance payment", result!.TransactionName);
        Assert.Equal(150.50m, result.Value);
    }

    [Fact]
    public async Task CreateTransaction_WithCategoryFromAnotherUser_ReturnsNull()
    {
        // Arrange
        var (service, categoryRepository) = CreateSut();
        var categoryFromOtherUser = await categoryRepository.CreateCategory(2, new CategoryRequest { Name = "Rent", Type = TransactionType.Saida });
        var request = new TransactionRequest
        {
            TransactionName = "Suspicious transaction",
            Value = 500m,
            Type = TransactionType.Saida,
            CategoryId = categoryFromOtherUser.CategoryId
        };

        // Act
        var result = await service.CreateTransaction(1, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllTransactions_ReturnsOnlyTransactionsForRequestedUser()
    {
        // Arrange
        var (service, _) = CreateSut();
        await service.CreateTransaction(1, new TransactionRequest
        {
            TransactionName = "Mine",
            Value = 10m,
            Type = TransactionType.Saida
        });
        await service.CreateTransaction(2, new TransactionRequest
        {
            TransactionName = "Not mine",
            Value = 20m,
            Type = TransactionType.Saida
        });

        // Act
        var result = await service.GetAllTransactions(1);

        // Assert
        var transaction = Assert.Single(result.TransactionResponses);
        Assert.Equal("Mine", transaction.TransactionName);
    }
}
