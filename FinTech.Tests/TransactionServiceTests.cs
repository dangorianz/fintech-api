using fintech_api.DTOs;
using fintech_api.Models;
using fintech_api.Repositories.Implementations;
using fintech_api.Repositories.Interfaces;
using fintech_api.Services;

namespace FinTech.Tests;

public sealed class TransactionServiceTests
{
    [Fact]
    public async Task Create_returns_original_transaction_for_repeated_idempotency_key()
    {
        var store = new InMemoryStore();
        ITransactionRepository repository = new InMemoryTransactionRepository(store);
        var service = new TransactionService(repository);
        var request = new CreateTransactionRequest(
            IdempotencyKey: "payment-button-double-click",
            Type: TransactionType.Payment,
            Amount: 120m,
            Description: "First payment");

        var first = await service.CreateAsync(request, CancellationToken.None);
        var second = await service.CreateAsync(request with { Amount = 999m, Description = "Duplicated click" }, CancellationToken.None);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(120m, second.Amount);
        Assert.Single(store.Transactions, transaction => transaction.IdempotencyKey == request.IdempotencyKey);
    }
}
