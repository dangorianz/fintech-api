using fintech_api.DTOs;

namespace fintech_api.Services.Interfaces;

public interface ILoanService
{
    Task<SimulateLoanResponse> SimulateAsync(SimulateLoanRequest request);
    Task<LoanResponse> CreateAsync(CreateLoanRequest request);
    Task<List<LoanResponse>> GetAllAsync(string? userId);
    Task<LoanResponse?> GetByIdAsync(Guid id);
    Task<List<PaymentScheduleResponse>> GetScheduleAsync(Guid id);
    Task<LoanResponse?> ApproveAsync(Guid id);
    Task<LoanResponse?> RejectAsync(Guid id);
}