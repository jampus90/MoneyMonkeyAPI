using Microsoft.AspNetCore.Identity;
using Moq;
using MoneyMonkey.Application.Services;
using MoneyMonkey.Communication.Enums;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Data.Entities;
using MoneyMonkey.Data.Repository;
using MoneyMonkey.Tests.TestHelpers;

namespace MoneyMonkey.Tests.Services;

public class UserServiceTests
{
    private static (UserService Service, UserRepository Repository, Mock<ITokenService> TokenServiceMock) CreateSut()
    {
        var context = DbContextFactory.CreateInMemoryContext();
        var repository = new UserRepository(context, new PasswordHasher<User>());
        var tokenServiceMock = new Mock<ITokenService>();
        var service = new UserService(repository, tokenServiceMock.Object);

        return (service, repository, tokenServiceMock);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsLoginResponseWithGeneratedToken()
    {
        // Arrange
        var (service, repository, tokenServiceMock) = CreateSut();
        var createdUser = await repository.CreateUser(new UserRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            UserType = UserType.Pf,
            Username = "ada",
            Password = "correct-password"
        });
        var expiresAt = DateTime.UtcNow.AddHours(1);
        tokenServiceMock
            .Setup(t => t.GenerateToken(It.Is<User>(u => u.UserId == createdUser.UserId)))
            .Returns(("fake-jwt-token", expiresAt));

        // Act
        var result = await service.Login(new LoginRequest { Username = "ada", Password = "correct-password" });

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fake-jwt-token", result!.Token);
        Assert.Equal(expiresAt, result.ExpiresAt);
        Assert.Equal(createdUser.UserId, result.UserId);
        Assert.Equal("Ada", result.FirstName);
        tokenServiceMock.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsNullAndDoesNotGenerateToken()
    {
        // Arrange
        var (service, repository, tokenServiceMock) = CreateSut();
        await repository.CreateUser(new UserRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            UserType = UserType.Pf,
            Username = "ada",
            Password = "correct-password"
        });

        // Act
        var result = await service.Login(new LoginRequest { Username = "ada", Password = "wrong-password" });

        // Assert
        Assert.Null(result);
        tokenServiceMock.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_DelegatesToRepositoryAndReturnsCreatedUser()
    {
        // Arrange
        var (service, _, _) = CreateSut();
        var request = new UserRequest
        {
            FirstName = "Grace",
            LastName = "Hopper",
            UserType = UserType.Staff,
            Username = "grace",
            Password = "password123"
        };

        // Act
        var result = await service.CreateUser(request);

        // Assert
        Assert.True(result.UserId > 0);
        Assert.Equal("Grace", result.FirstName);
        Assert.Equal(UserType.Staff, result.UserType);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsUsersFromRepository()
    {
        // Arrange
        var (service, repository, _) = CreateSut();
        await repository.CreateUser(new UserRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            UserType = UserType.Pf,
            Username = "ada",
            Password = "password123"
        });

        // Act
        var result = await service.GetAllUsers();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result!.UserResponses);
    }
}
