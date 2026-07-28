using MoneyMonkey.Communication.Enums;

namespace MoneyMonkey.Data.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}
