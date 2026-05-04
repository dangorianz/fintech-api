using fintech_api.Data;
using fintech_api.Models;
using fintech_api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace fintech_api.Repositories.Implementations;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _context.Transactions.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey)
    {
        return await _context.Transactions.FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey);
    }

    public async Task<List<Transaction>> GetAllAsync(TransactionType? type, TransactionStatus? status)
    {
        var query = _context.Transactions.AsQueryable();

        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }
}