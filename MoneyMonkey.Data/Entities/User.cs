using MoneyMonkey.Communication.Enums;

namespace MoneyMonkey.Data.Entities;

public class User
{
    public long UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserType Type { get; set; }
}
