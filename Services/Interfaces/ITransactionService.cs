using fintech_api.DTOs;
using fintech_api.Models;

namespace fintech_api.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponse> CreateAsync(CreateTransactionRequest request);
    Task<List<TransactionResponse>> GetAllAsync(TransactionType? type, TransactionStatus? status);
    Task<TransactionResponse?> GetByIdAsync(Guid id);
}