using Microsoft.EntityFrameworkCore;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Communication.Response;
using MoneyMonkey.Data.Entities;

namespace MoneyMonkey.Data.Repository;
public class TransactionRepository
{
    private readonly MoneyMonkeyDbContext _context;

    public TransactionRepository(MoneyMonkeyDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionResponseList> GetAllTransactions(long userId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new TransactionResponse
            {
                TransactionId = t.TransactionId,
                TransactionName = t.TransactionName,
                Value = t.Value,
                Type = t.Type,
                PaymentMethod = t.PaymentMethod,
                CategoryId = t.CategoryId,
                TransactionDate = t.TransactionDate
            })
            .ToListAsync();

        return new TransactionResponseList { TransactionResponses = transactions };
    }

    public async Task<TransactionResponse?> CreateTransaction(long userId, TransactionRequest request)
    {
        if (request.CategoryId is not null)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryId == request.CategoryId && c.UserId == userId);

            if (!categoryExists)
            {
                return null;
            }
        }

        var transaction = new Transaction
        {
            UserId = userId,
            TransactionName = request.TransactionName,
            Value = request.Value,
            Type = request.Type,
            PaymentMethod = request.PaymentMethod,
            CategoryId = request.CategoryId,
            TransactionDate = request.TransactionDate ?? DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return new TransactionResponse
        {
            TransactionId = transaction.TransactionId,
            TransactionName = transaction.TransactionName,
            Value = transaction.Value,
            Type = transaction.Type,
            PaymentMethod = transaction.PaymentMethod,
            CategoryId = transaction.CategoryId,
            TransactionDate = transaction.TransactionDate
        };
    }
}
