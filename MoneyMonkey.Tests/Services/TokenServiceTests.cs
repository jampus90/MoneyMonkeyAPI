using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using MoneyMonkey.Application.Services;
using MoneyMonkey.Application.Settings;
using MoneyMonkey.Communication.Enums;
using MoneyMonkey.Data.Entities;

namespace MoneyMonkey.Tests.Services;

public class TokenServiceTests
{
    private static TokenService CreateSut(int expirationMinutes = 60)
    {
        var settings = new JwtSettings
        {
            Secret = "this-is-a-sufficiently-long-test-signing-secret",
            Issuer = "MoneyMonkey.Tests",
            Audience = "MoneyMonkey.Tests.Audience",
            ExpirationMinutes = expirationMinutes
        };

        return new TokenService(Options.Create(settings));
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsTokenContainingExpectedClaims()
    {
        // Arrange
        var tokenService = CreateSut();
        var user = new User { UserId = 42, FirstName = "Ada", LastName = "Lovelace", Type = UserType.Admin };

        // Act
        var (token, _) = tokenService.GenerateToken(user);

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("42", jwt.Subject);
        Assert.Equal("Ada Lovelace", jwt.Claims.Single(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal("Admin", jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void GenerateToken_WithConfiguredExpiration_ReturnsExpiryMatchingConfiguredMinutes()
    {
        // Arrange
        var tokenService = CreateSut(expirationMinutes: 30);
        var user = new User { UserId = 1, FirstName = "Alan", LastName = "Turing", Type = UserType.Staff };
        var before = DateTime.UtcNow;

        // Act
        var (_, expiresAt) = tokenService.GenerateToken(user);

        // Assert
        Assert.InRange(expiresAt, before.AddMinutes(30).AddSeconds(-5), before.AddMinutes(30).AddSeconds(5));
    }

    [Fact]
    public void GenerateToken_CalledTwiceForSameUser_ReturnsDifferentTokens()
    {
        // Arrange
        var tokenService = CreateSut();
        var user = new User { UserId = 7, FirstName = "Grace", LastName = "Hopper", Type = UserType.Pf };

        // Act
        var (firstToken, _) = tokenService.GenerateToken(user);
        var (secondToken, _) = tokenService.GenerateToken(user);

        // Assert
        Assert.NotEqual(firstToken, secondToken);
    }
}
