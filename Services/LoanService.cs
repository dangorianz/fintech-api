using fintech_api.DTOs;
using fintech_api.Models;
using fintech_api.Repositories.Interfaces;
using fintech_api.Services.Interfaces;
using fintech_api.Utils;

namespace fintech_api.Services;

public class LoanService : ILoanService
{
    private const decimal InterestRate = 24m;
    private const decimal MinTea = 18m;
    private const decimal MaxTea = 35m;

    private readonly ILoanRepository _loanRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly FinancialCalculator _calculator;

    public LoanService(
        ILoanRepository loanRepository,
        ITransactionRepository transactionRepository,
        FinancialCalculator calculator)
    {
        _loanRepository = loanRepository;
        _transactionRepository = transactionRepository;
        _calculator = calculator;
    }

    public Task<SimulateLoanResponse> SimulateAsync(SimulateLoanRequest request)
    {
        ValidateLoanRules(request.Amount, request.Term);
        var annualRate = DetermineInterestRate(request.InterestRate, request.Amount);

        var monthlyPayment = _calculator.CalculateMonthlyPayment(request.Amount, request.Term, annualRate, request.LoanType);
        var schedule = _calculator.GenerateSchedule(request.Amount, request.Term, annualRate, request.LoanType, DateTime.UtcNow);

        return Task.FromResult(new SimulateLoanResponse
        {
            Amount = request.Amount,
            Term = request.Term,
            InterestRate = annualRate,
            MonthlyPayment = monthlyPayment,
            LoanType = request.LoanType,
            Schedule = schedule
        });
    }

    public async Task<LoanResponse> CreateAsync(CreateLoanRequest request)
    {
        ValidateLoanRules(request.Amount, request.Term);

        if (request.MonthlyIncome <= 0)
            throw new Exception("El ingreso mensual es requerido y debe ser mayor que 0.");

        var annualRate = DetermineInterestRate(request.InterestRate, request.Amount);

        var activeLoans = await _loanRepository.CountActiveLoansByUserAsync(request.UserId);

        if (activeLoans >= 3)
            throw new Exception("El cliente no puede tener más de 3 préstamos activos.");

        var currentMonthlyPayments = await _loanRepository.SumActiveMonthlyPaymentsByUserAsync(request.UserId);
        var monthlyPayment = _calculator.CalculateMonthlyPayment(request.Amount, request.Term, annualRate, request.LoanType);

        if (currentMonthlyPayments + monthlyPayment > request.MonthlyIncome * 0.40m)
            throw new Exception("La suma de cuotas no puede exceder el 40% de los ingresos mensuales.");

        var loan = new Loan
        {
            UserId = request.UserId,
            Amount = request.Amount,
            Term = request.Term,
            InterestRate = annualRate,
            LoanType = request.LoanType,
            MonthlyPayment = monthlyPayment,
            MonthlyIncome = request.MonthlyIncome,
            Status = request.Amount < 10000 && activeLoans < 2
                ? LoanStatus.Approved
                : LoanStatus.Pending
        };

        var schedule = _calculator.GenerateSchedule(request.Amount, request.Term, annualRate, request.LoanType, DateTime.UtcNow);

        loan.PaymentSchedules = schedule.Select(x => new PaymentSchedule
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

        var created = await _loanRepository.CreateAsync(loan);

        if (created.Status == LoanStatus.Approved)
        {
            await _transactionRepository.CreateAsync(new Transaction
            {
                IdempotencyKey = $"DISBURSEMENT-{created.Id}",
                Type = TransactionType.Disbursement,
                Amount = created.Amount,
                Status = TransactionStatus.Completed,
                LoanId = created.Id,
                Description = "Desembolso automático de préstamo aprobado"
            });
        }

        return MapLoan(created);
    }

    public async Task<List<LoanResponse>> GetAllAsync(string? userId)
    {
        var loans = await _loanRepository.GetAllAsync(userId);
        return loans.Select(MapLoan).ToList();
    }

    public async Task<LoanResponse?> GetByIdAsync(Guid id)
    {
        var loan = await _loanRepository.GetByIdAsync(id);
        return loan == null ? null : MapLoan(loan);
    }

    public async Task<List<PaymentScheduleResponse>> GetScheduleAsync(Guid id)
    {
        var loan = await _loanRepository.GetByIdAsync(id);

        if (loan == null)
            return new List<PaymentScheduleResponse>();

        if (!loan.PaymentSchedules.Any())
            return _calculator.GenerateSchedule(
                loan.Amount,
                loan.Term,
                loan.InterestRate,
                loan.LoanType,
                loan.CreatedAt);

        return loan.PaymentSchedules
            .OrderBy(x => x.PaymentNumber)
            .Select(x => new PaymentScheduleResponse
            {
                PaymentNumber = x.PaymentNumber,
                DueDate = x.DueDate,
                TotalPayment = x.TotalPayment,
                Principal = x.Principal,
                Interest = x.Interest,
                RemainingBalance = x.RemainingBalance,
                Status = x.Status
            })
            .ToList();
    }

    public async Task<LoanResponse?> ApproveAsync(Guid id)
    {
        var loan = await _loanRepository.GetByIdAsync(id);

        if (loan == null)
            return null;

        loan.Status = LoanStatus.Approved;

        await _loanRepository.UpdateAsync(loan);

        var existing = await _transactionRepository.GetByIdempotencyKeyAsync($"DISBURSEMENT-{loan.Id}");

        if (existing == null)
        {
            await _transactionRepository.CreateAsync(new Transaction
            {
                IdempotencyKey = $"DISBURSEMENT-{loan.Id}",
                Type = TransactionType.Disbursement,
                Amount = loan.Amount,
                Status = TransactionStatus.Completed,
                LoanId = loan.Id,
                Description = "Desembolso de préstamo aprobado"
            });
        }

        return MapLoan(loan);
    }

    public async Task<LoanResponse?> RejectAsync(Guid id)
    {
        var loan = await _loanRepository.GetByIdAsync(id);

        if (loan == null)
            return null;

        loan.Status = LoanStatus.Rejected;
        await _loanRepository.UpdateAsync(loan);

        return MapLoan(loan);
    }

    private static void ValidateLoanRules(decimal amount, int term)
    {
        if (amount < 500 || amount > 50000)
            throw new Exception("El monto debe estar entre 500 y 50000.");

        if (term < 6 || term > 60)
            throw new Exception("El plazo debe estar entre 6 y 60 meses.");
    }

    private static decimal DetermineInterestRate(decimal? requestedRate, decimal amount)
    {
        if (requestedRate.HasValue)
        {
            if (requestedRate.Value < MinTea || requestedRate.Value > MaxTea)
                throw new Exception($"La tasa efectiva anual (TEA) debe estar entre {MinTea}% y {MaxTea}%.");

            return requestedRate.Value;
        }

        // Simple tiered TEA based on amount
        if (amount < 10000) return 18m;
        if (amount < 30000) return 24m;
        return 30m;
    }

    private static LoanResponse MapLoan(Loan loan)
    {
        return new LoanResponse
        {
            Id = loan.Id,
            UserId = loan.UserId,
            Amount = loan.Amount,
            Term = loan.Term,
            InterestRate = loan.InterestRate,
            LoanType = loan.LoanType,
            Status = loan.Status,
            MonthlyPayment = loan.MonthlyPayment,
            CreatedAt = loan.CreatedAt
        };
    }
}
