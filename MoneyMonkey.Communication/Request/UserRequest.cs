using System.ComponentModel.DataAnnotations;
using MoneyMonkey.Communication.Enums;

namespace MoneyMonkey.Communication.Request;
public class UserRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required UserType UserType { get; set; }
    [StringLength(20)]
    public required string Username { get; set; }
    public required string Password { get; set; }
}
