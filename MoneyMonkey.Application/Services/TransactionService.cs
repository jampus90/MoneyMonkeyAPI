using MoneyMonkey.Communication.Request;
using MoneyMonkey.Communication.Response;
using MoneyMonkey.Data.Repository;

namespace MoneyMonkey.Application.Services;
public class TransactionService
{
    private readonly TransactionRepository _transactionRepository;

    public TransactionService(TransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<TransactionResponseList> GetAllTransactions(long userId)
    {
        return await _transactionRepository.GetAllTransactions(userId);
    }

    public async Task<TransactionResponse?> CreateTransaction(long userId, TransactionRequest request)
    {
        return await _transactionRepository.CreateTransaction(userId, request);
    }
}
