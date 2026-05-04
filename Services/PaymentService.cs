using fintech_api.Data;
using fintech_api.DTOs;
using fintech_api.Models;
using fintech_api.Services.Interfaces;
using fintech_api.Utils;
using Microsoft.EntityFrameworkCore;

namespace fintech_api.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly FinancialCalculator _calculator;

    public PaymentService(ApplicationDbContext context, FinancialCalculator calculator)
    {
        _context = context;
        _calculator = calculator;
    }

    public async Task<PayNextInstallmentResponse> PayNextInstallmentAsync(
        Guid loanId,
        PayNextInstallmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new Exception("La llave de idempotencia es requerida.");

        await using var dbTransaction = await _context.Database.BeginTransactionAsync();

        var existing = await _context.Transactions
            .FirstOrDefaultAsync(x => x.IdempotencyKey == request.IdempotencyKey);

        if (existing != null)
        {
            var existingPayment = await GetLatestPaidScheduleAsync(loanId);
            await dbTransaction.CommitAsync();

            return new PayNextInstallmentResponse
            {
                Transaction = MapTransaction(existing),
                Payment = existingPayment == null ? new PaymentScheduleResponse() : MapPayment(existingPayment),
                RemainingPendingPayments = await CountPendingPaymentsAsync(loanId)
            };
        }

        var loan = await _context.Loans
            .Include(x => x.PaymentSchedules)
            .FirstOrDefaultAsync(x => x.Id == loanId);

        if (loan == null)
            throw new Exception("Prestamo no encontrado.");

        if (loan.Status is LoanStatus.Rejected or LoanStatus.Pending)
            throw new Exception("Solo se pueden pagar prestamos aprobados o activos.");

        if (!loan.PaymentSchedules.Any())
        {
            var generated = _calculator.GenerateSchedule(
                loan.Amount,
                loan.Term,
                loan.InterestRate,
                loan.LoanType,
                loan.CreatedAt);

            loan.PaymentSchedules = generated.Select(x => new PaymentSchedule
            {
                LoanId = loan.Id,
                PaymentNumber = x.PaymentNumber,
                DueDate = x.DueDate,
                TotalPayment = x.TotalPayment,
                Principal = x.Principal,
                Interest = x.Interest,
                RemainingBalance = x.RemainingBalance,
                Status = PaymentScheduleStatus.Pending
            }).ToList();

            _context.PaymentSchedules.AddRange(loan.PaymentSchedules);
            await _context.SaveChangesAsync();
        }

        var nextPayment = loan.PaymentSchedules
            .Where(x => x.Status == PaymentScheduleStatus.Pending)
            .OrderBy(x => x.PaymentNumber)
            .FirstOrDefault();

        if (nextPayment == null)
            throw new Exception("El prestamo no tiene cuotas pendientes.");

        nextPayment.Status = PaymentScheduleStatus.Paid;

        var transaction = new Transaction
        {
            IdempotencyKey = request.IdempotencyKey,
            Type = TransactionType.Payment,
            Amount = nextPayment.TotalPayment,
            Status = TransactionStatus.Completed,
            LoanId = loan.Id,
            Description = $"Pago de cuota #{nextPayment.PaymentNumber}"
        };

        _context.Transactions.Add(transaction);

        if (!loan.PaymentSchedules.Any(x => x.Status == PaymentScheduleStatus.Pending))
            loan.Status = LoanStatus.Active;

        loan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await dbTransaction.CommitAsync();

        return new PayNextInstallmentResponse
        {
            Transaction = MapTransaction(transaction),
            Payment = MapPayment(nextPayment),
            RemainingPendingPayments = loan.PaymentSchedules.Count(x => x.Status == PaymentScheduleStatus.Pending)
        };
    }

    private async Task<PaymentSchedule?> GetLatestPaidScheduleAsync(Guid loanId)
    {
        return await _context.PaymentSchedules
            .Where(x => x.LoanId == loanId && x.Status == PaymentScheduleStatus.Paid)
            .OrderByDescending(x => x.PaymentNumber)
            .FirstOrDefaultAsync();
    }

    private async Task<int> CountPendingPaymentsAsync(Guid loanId)
    {
        return await _context.PaymentSchedules.CountAsync(x =>
            x.LoanId == loanId && x.Status == PaymentScheduleStatus.Pending);
    }

    private static PaymentScheduleResponse MapPayment(PaymentSchedule payment)
    {
        return new PaymentScheduleResponse
        {
            PaymentNumber = payment.PaymentNumber,
            DueDate = payment.DueDate,
            TotalPayment = payment.TotalPayment,
            Principal = payment.Principal,
            Interest = payment.Interest,
            RemainingBalance = payment.RemainingBalance,
            Status = payment.Status
        };
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
