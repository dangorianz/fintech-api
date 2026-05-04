using fintech_api.Models;

namespace fintech_api.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction> CreateAsync(Transaction transaction);
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey);
    Task<List<Transaction>> GetAllAsync(TransactionType? type, TransactionStatus? status);
}