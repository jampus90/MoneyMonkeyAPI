using MoneyMonkey.Data.Entities;

namespace MoneyMonkey.Application.Services;
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
