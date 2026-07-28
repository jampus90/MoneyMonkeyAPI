using MoneyMonkey.Communication.Enums;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Data.Repository;
using MoneyMonkey.Tests.TestHelpers;

namespace MoneyMonkey.Tests.Repository;

public class CategoryRepositoryTests
{
    [Fact]
    public async Task CreateCategory_WithValidRequest_PersistsCategoryForUser()
    {
        // Arrange
        var context = DbContextFactory.CreateInMemoryContext();
        var repository = new CategoryRepository(context);
        const long userId = 1;
        var request = new CategoryRequest { Name = "Groceries", Type = TransactionType.Saida };

        // Act
        var result = await repository.CreateCategory(userId, request);

        // Assert
        Assert.True(result.CategoryId > 0);
        Assert.Equal("Groceries", result.Name);
        Assert.Equal(TransactionType.Saida, result.Type);
        Assert.Equal(userId, Assert.Single(context.Categories).UserId);
    }

    [Fact]
    public async Task GetAllCategories_ReturnsOnlyCategoriesBelongingToRequestedUser()
    {
        // Arrange
        var context = DbContextFactory.CreateInMemoryContext();
        var repository = new CategoryRepository(context);
        await repository.CreateCategory(1, new CategoryRequest { Name = "Salary", Type = TransactionType.Entrada });
        await repository.CreateCategory(2, new CategoryRequest { Name = "Rent", Type = TransactionType.Saida });

        // Act
        var result = await repository.GetAllCategories(1);

        // Assert
        var category = Assert.Single(result.CategoryResponses);
        Assert.Equal("Salary", category.Name);
    }

    [Fact]
    public async Task GetAllCategories_WhenUserHasNoCategories_ReturnsEmptyList()
    {
        // Arrange
        var context = DbContextFactory.CreateInMemoryContext();
        var repository = new CategoryRepository(context);

        // Act
        var result = await repository.GetAllCategories(99);

        // Assert
        Assert.Empty(result.CategoryResponses);
    }
}
