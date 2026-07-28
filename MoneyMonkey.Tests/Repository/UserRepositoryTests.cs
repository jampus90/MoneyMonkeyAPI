using Microsoft.AspNetCore.Identity;
using MoneyMonkey.Communication.Enums;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Data.Entities;
using MoneyMonkey.Data.Repository;
using MoneyMonkey.Tests.TestHelpers;

namespace MoneyMonkey.Tests.Repository;

public class UserRepositoryTests
{
    private static UserRepository CreateSut(out MoneyMonkey.Data.MoneyMonkeyDbContext context)
    {
        context = DbContextFactory.CreateInMemoryContext();
        return new UserRepository(context, new PasswordHasher<User>());
    }

    [Fact]
    public async Task CreateUser_WithValidRequest_PersistsUserAndHashedCredential()
    {
        // Arrange
        var repository = CreateSut(out var context);
        var request = new UserRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            UserType = UserType.Pf,
            Username = "ada",
            Password = "plain-text-password"
        };

        // Act
        var result = await repository.CreateUser(request);

        // Assert
        Assert.True(result.UserId > 0);
        Assert.Equal("Ada", result.FirstName);
        Assert.Equal("Lovelace", result.LastName);
        Assert.Equal(UserType.Pf, result.UserType);

        var storedCredential = Assert.Single(context.Credentials);
        Assert.Equal("ada", storedCredential.Username);
        Assert.NotEqual("plain-text-password", storedCredential.Password);
    }

    [Fact]
    public async Task GetAllUsers_WithMultipleUsers_ReturnsAllOfThem()
    {
        // Arrange
        var repository = CreateSut(out var context);
        await repository.CreateUser(new UserRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            UserType = UserType.Pf,
            Username = "ada",
            Password = "password1"
        });
        await repository.CreateUser(new UserRequest
        {
            FirstName = "Alan",
            LastName = "Turing",
            UserType = UserType.Admin,
            Username = "alan",
            Password = "password2"
        });

        // Act
        var result = await repository.GetAllUsers();

        // Assert
        Assert.Equal(2, result.UserResponses.Count);
        Assert.Contains(result.UserResponses, u => u.FirstName == "Ada");
        Assert.Contains(result.UserResponses, u => u.FirstName == "Alan");
    }

    [Fact]
    public async Task Authenticate_WithCorrectCredentials_ReturnsUser()
    {
        // Arrange
        var repository = CreateSut(out _);
        var created = await repository.CreateUser(new UserRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            UserType = UserType.Pf,
            Username = "ada",
            Password = "correct-password"
        });

        // Act
        var result = await repository.Authenticate("ada", "correct-password");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.UserId, result!.UserId);
    }

    [Fact]
    public async Task Authenticate_WithWrongPassword_ReturnsNull()
    {
        // Arrange
        var repository = CreateSut(out _);
        await repository.CreateUser(new UserRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            UserType = UserType.Pf,
            Username = "ada",
            Password = "correct-password"
        });

        // Act
        var result = await repository.Authenticate("ada", "wrong-password");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Authenticate_WithUnknownUsername_ReturnsNull()
    {
        // Arrange
        var repository = CreateSut(out _);

        // Act
        var result = await repository.Authenticate("does-not-exist", "any-password");

        // Assert
        Assert.Null(result);
    }
}
