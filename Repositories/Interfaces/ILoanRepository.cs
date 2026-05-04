using fintech_api.Models;

namespace fintech_api.Repositories.Interfaces;

public interface ILoanRepository
{
    Task<Loan> CreateAsync(Loan loan);
    Task<List<Loan>> GetAllAsync(string? userId);
    Task<Loan?> GetByIdAsync(Guid id);
    Task<int> CountActiveLoansByUserAsync(string userId);
    Task<decimal> SumActiveMonthlyPaymentsByUserAsync(string userId);
    Task UpdateAsync(Loan loan);
}