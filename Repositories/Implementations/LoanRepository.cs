using fintech_api.Data;
using fintech_api.Models;
using fintech_api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace fintech_api.Repositories.Implementations;

public class LoanRepository : ILoanRepository
{
    private readonly ApplicationDbContext _context;

    public LoanRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Loan> CreateAsync(Loan loan)
    {
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();
        return loan;
    }

    public async Task<List<Loan>> GetAllAsync(string? userId)
    {
        var query = _context.Loans.AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(x => x.UserId == userId);

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<Loan?> GetByIdAsync(Guid id)
    {
        return await _context.Loans
            .Include(x => x.PaymentSchedules)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<int> CountActiveLoansByUserAsync(string userId)
    {
        return await _context.Loans.CountAsync(x =>
            x.UserId == userId &&
            (x.Status == LoanStatus.Active || x.Status == LoanStatus.Approved));
    }

    public async Task<decimal> SumActiveMonthlyPaymentsByUserAsync(string userId)
    {
        return await _context.Loans
            .Where(x => x.UserId == userId &&
                        (x.Status == LoanStatus.Active || x.Status == LoanStatus.Approved))
            .SumAsync(x => x.MonthlyPayment);
    }

    public async Task UpdateAsync(Loan loan)
    {
        loan.UpdatedAt = DateTime.UtcNow;
        _context.Loans.Update(loan);
        await _context.SaveChangesAsync();
    }
}