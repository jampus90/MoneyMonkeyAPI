namespace MoneyMonkey.Data.Entities;

public class Credential
{
    public int CredentialId { get; set; }
    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
