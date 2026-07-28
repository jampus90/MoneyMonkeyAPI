using MoneyMonkey.Application.Services;
using MoneyMonkey.Communication.Enums;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Data.Repository;
using MoneyMonkey.Tests.TestHelpers;

namespace MoneyMonkey.Tests.Services;

public class CategoryServiceTests
{
    private static CategoryService CreateSut()
    {
        var context = DbContextFactory.CreateInMemoryContext();
        var repository = new CategoryRepository(context);
        return new CategoryService(repository);
    }

    [Fact]
    public async Task CreateCategory_WithValidRequest_ReturnsCreatedCategoryForUser()
    {
        // Arrange
        var service = CreateSut();
        const long userId = 1;
        var request = new CategoryRequest { Name = "Groceries", Type = TransactionType.Saida };

        // Act
        var result = await service.CreateCategory(userId, request);

        // Assert
        Assert.True(result.CategoryId > 0);
        Assert.Equal("Groceries", result.Name);
        Assert.Equal(TransactionType.Saida, result.Type);
    }

    [Fact]
    public async Task GetAllCategories_ReturnsOnlyCategoriesForRequestedUser()
    {
        // Arrange
        var service = CreateSut();
        await service.CreateCategory(1, new CategoryRequest { Name = "Salary", Type = TransactionType.Entrada });
        await service.CreateCategory(2, new CategoryRequest { Name = "Rent", Type = TransactionType.Saida });

        // Act
        var result = await service.GetAllCategories(1);

        // Assert
        var category = Assert.Single(result.CategoryResponses);
        Assert.Equal("Salary", category.Name);
    }
}
