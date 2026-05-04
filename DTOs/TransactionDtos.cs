using fintech_api.Models;

namespace fintech_api.DTOs;

public class CreateTransactionRequest
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public Guid? LoanId { get; set; }
    public string? Description { get; set; }
}

public class TransactionResponse
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public TransactionStatus Status { get; set; }
    public Guid? LoanId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PayNextInstallmentRequest
{
    public string IdempotencyKey { get; set; } = string.Empty;
}

public class PayNextInstallmentResponse
{
    public TransactionResponse Transaction { get; set; } = new();
    public PaymentScheduleResponse Payment { get; set; } = new();
    public int RemainingPendingPayments { get; set; }
}
