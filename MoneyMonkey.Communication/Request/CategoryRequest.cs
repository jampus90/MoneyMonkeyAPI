using System.ComponentModel.DataAnnotations;
using MoneyMonkey.Communication.Enums;

namespace MoneyMonkey.Communication.Request;
public class CategoryRequest
{
    [StringLength(50)]
    public required string Name { get; set; }
    public required TransactionType Type { get; set; }
}
