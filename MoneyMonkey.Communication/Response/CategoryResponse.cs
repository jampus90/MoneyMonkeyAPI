using MoneyMonkey.Communication.Enums;

namespace MoneyMonkey.Communication.Response;
public class CategoryResponse
{
    public int CategoryId { get; set; }
    public string? Name { get; set; }
    public TransactionType Type { get; set; }
}
