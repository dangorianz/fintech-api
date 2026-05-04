namespace fintech_api.Models;

public class Loan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public int Term { get; set; }

    public decimal InterestRate { get; set; }

    public LoanType LoanType { get; set; }

    public LoanStatus Status { get; set; }

    public decimal MonthlyPayment { get; set; }

    public decimal MonthlyIncome { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<PaymentSchedule> PaymentSchedules { get; set; } = new();
}