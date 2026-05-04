using fintech_api.DTOs;
using fintech_api.Models;
using fintech_api.Repositories.Interfaces;
using fintech_api.Services.Interfaces;

namespace fintech_api.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<TransactionResponse> CreateAsync(CreateTransactionRequest request)
    {
        var existing = await _transactionRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey);

        if (existing != null)
            return MapTransaction(existing);

        var transaction = new Transaction
        {
            IdempotencyKey = request.IdempotencyKey,
            Type = request.Type,
            Amount = request.Amount,
            LoanId = request.LoanId,
            Description = request.Description,
            Status = TransactionStatus.Completed
        };

        var created = await _transactionRepository.CreateAsync(transaction);

        return MapTransaction(created);
    }

    public async Task<List<TransactionResponse>> GetAllAsync(TransactionType? type, TransactionStatus? status)
    {
        var transactions = await _transactionRepository.GetAllAsync(type, status);
        return transactions.Select(MapTransaction).ToList();
    }

    public async Task<TransactionResponse?> GetByIdAsync(Guid id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        return transaction == null ? null : MapTransaction(transaction);
    }

    private static TransactionResponse MapTransaction(Transaction transaction)
    {
        return new TransactionResponse
        {
            Id = transaction.Id,
            IdempotencyKey = transaction.IdempotencyKey,
            Type = transaction.Type,
            Amount = transaction.Amount,
            Status = transaction.Status,
            LoanId = transaction.LoanId,
            Description = transaction.Description,
            CreatedAt = transaction.CreatedAt
        };
    }
}