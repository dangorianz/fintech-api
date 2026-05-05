using fintech_api.Models;
using System.ComponentModel.DataAnnotations;

namespace fintech_api.DTOs;

public class SimulateLoanRequest
{
    [Required]
    [Range(500, 50000)]
    public decimal Amount { get; set; }

    [Required]
    [Range(6, 60)]
    public int Term { get; set; }

    [Required]
    public LoanType LoanType { get; set; } = LoanType.Fixed;

    // Optional: allow client to request a specific annual interest rate (TEA)
    [Range(0, 100)]
    public decimal? InterestRate { get; set; }
}

public class CreateLoanRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Range(500, 50000)]
    public decimal Amount { get; set; }

    [Required]
    [Range(6, 60)]
    public int Term { get; set; }

    [Required]
    [Range(0, 1000000)]
    public decimal MonthlyIncome { get; set; }

    [Required]
    public LoanType LoanType { get; set; } = LoanType.Fixed;

    // Optional interest hint (annual percent). Backend will enforce bounds.
    [Range(0, 100)]
    public decimal? InterestRate { get; set; }
}

public class LoanResponse
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Term { get; set; }
    public decimal InterestRate { get; set; }
    public LoanType LoanType { get; set; }
    public LoanStatus Status { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentScheduleResponse
{
    public int PaymentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal Principal { get; set; }
    public decimal Interest { get; set; }
    public decimal RemainingBalance { get; set; }
    public PaymentScheduleStatus Status { get; set; }
}

public class SimulateLoanResponse
{
    public decimal Amount { get; set; }
    public int Term { get; set; }
    public decimal InterestRate { get; set; }
    public decimal MonthlyPayment { get; set; }
    public LoanType LoanType { get; set; }
    public List<PaymentScheduleResponse> Schedule { get; set; } = new();
}
