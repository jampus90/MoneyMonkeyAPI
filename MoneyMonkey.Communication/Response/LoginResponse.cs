using MoneyMonkey.Communication.Enums;

namespace MoneyMonkey.Communication.Response;
public class LoginResponse
{
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public long UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserType UserType { get; set; }
}
