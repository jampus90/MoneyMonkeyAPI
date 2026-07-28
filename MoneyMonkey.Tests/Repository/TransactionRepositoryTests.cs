using MoneyMonkey.Communication.Enums;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Data.Repository;
using MoneyMonkey.Tests.TestHelpers;

namespace MoneyMonkey.Tests.Repository;

public class TransactionRepositoryTests
{
    [Fact]
    public async Task CreateTransaction_WithoutCategory_PersistsTransactionUsingTodayAsDefaultDate()
    {
        // Arrange
        var context = DbContextFactory.CreateInMemoryContext();
        var repository = new TransactionRepository(context);
        const long userId = 1;
        var request = new TransactionRequest
        {
            TransactionName = "Freelance payment",
            Value = 150.50m,
            Type = TransactionType.Entrada,
            PaymentMethod = PaymentMethod.Pix,
            CategoryId = null,
            TransactionDate = null
        };

        // Act
        var result = await repository.CreateTransaction(userId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Freelance payment", result!.TransactionName);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), result.TransactionDate);
    }

    [Fact]
    public async Task CreateTransaction_WithCategoryOwnedByUser_PersistsTransaction()
    {
        // Arrange
        var context = DbContextFactory.CreateInMemoryContext();
        var categoryRepository = new CategoryRepository(context);
        var transactionRepository = new TransactionRepository(context);
        const long userId = 1;
        var category = await categoryRepository.CreateCategory(userId, new CategoryRequest { Name = "Salary", Type = TransactionType.Entrada });

        var request = new TransactionRequest
        {
            TransactionName = "Monthly salary",
            Value = 3000m,
            Type = TransactionType.Entrada,
            PaymentMethod = PaymentMethod.Transferencia,
            CategoryId = category.CategoryId,
            TransactionDate = new DateOnly(2026, 7, 1)
        };

        // Act
        var result = await transactionRepository.CreateTransaction(userId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.CategoryId, result!.CategoryId);
        Assert.Equal(new DateOnly(2026, 7, 1), result.TransactionDate);
    }

    [Fact]
    public async Task CreateTransaction_WithCategoryBelongingToAnotherUser_ReturnsNull()
    {
        // Arrange
        var context = DbContextFactory.CreateInMemoryContext();
        var categoryRepository = new CategoryRepository(context);
        var transactionRepository = new TransactionRepository(context);
        var categoryFromOtherUser = await categoryRepository.CreateCategory(2, new CategoryRequest { Name = "Rent", Type = TransactionType.Saida });

        var request = new TransactionRequest
        {
            TransactionName = "Suspicious transaction",
            Value = 500m,
            Type = TransactionType.Saida,
            PaymentMethod = PaymentMethod.Boleto,
            CategoryId = categoryFromOtherUser.CategoryId,
            TransactionDate = null
        };

        // Act
        var result = await transactionRepository.CreateTransaction(1, request);

        // Assert
        Assert.Null(result);
        Assert.Empty(context.Transactions);
    }

    [Fact]
    public async Task GetAllTransactions_ReturnsOnlyRequestedUserTransactionsOrderedByDateDescending()
    {
        // Arrange
        var context = DbContextFactory.CreateInMemoryContext();
        var repository = new TransactionRepository(context);
        await repository.CreateTransaction(1, new TransactionRequest
        {
            TransactionName = "Older",
            Value = 10m,
            Type = TransactionType.Saida,
            TransactionDate = new DateOnly(2026, 1, 1)
        });
        await repository.CreateTransaction(1, new TransactionRequest
        {
            TransactionName = "Newer",
            Value = 20m,
            Type = TransactionType.Saida,
            TransactionDate = new DateOnly(2026, 6, 1)
        });
        await repository.CreateTransaction(2, new TransactionRequest
        {
            TransactionName = "Other user",
            Value = 30m,
            Type = TransactionType.Saida,
            TransactionDate = new DateOnly(2026, 3, 1)
        });

        // Act
        var result = await repository.GetAllTransactions(1);

        // Assert
        Assert.Equal(2, result.TransactionResponses.Count);
        Assert.Equal("Newer", result.TransactionResponses[0].TransactionName);
        Assert.Equal("Older", result.TransactionResponses[1].TransactionName);
    }
}
