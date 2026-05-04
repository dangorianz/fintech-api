using fintech_api.DTOs;

namespace fintech_api.Services.Interfaces;

public interface IPaymentService
{
    Task<PayNextInstallmentResponse> PayNextInstallmentAsync(Guid loanId, PayNextInstallmentRequest request);
}
