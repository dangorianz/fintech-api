using fintech_api.DTOs;
using fintech_api.Models;

namespace fintech_api.Utils;

public class FinancialCalculator
{
    public decimal CalculateMonthlyPayment(decimal amount, int term, decimal annualRate)
    {
        var monthlyRate = (decimal)Math.Pow((double)(1 + annualRate / 100), 1.0 / 12.0) - 1;

        if (monthlyRate == 0)
            return Math.Round(amount / term, 2);

        var factor = (decimal)Math.Pow((double)(1 + monthlyRate), term);

        var payment = amount * ((monthlyRate * factor) / (factor - 1));

        return Math.Round(payment, 2);
    }

    public decimal CalculateMonthlyPayment(decimal amount, int term, decimal annualRate, LoanType loanType)
    {
        return loanType == LoanType.Decreasing
            ? CalculateFirstDecreasingPayment(amount, term, annualRate)
            : CalculateMonthlyPayment(amount, term, annualRate);
    }

    public List<PaymentScheduleResponse> GenerateSchedule(
        decimal amount,
        int term,
        decimal annualRate,
        LoanType loanType,
        DateTime startDate)
    {
        return loanType == LoanType.Decreasing
            ? GenerateDecreasingSchedule(amount, term, annualRate, startDate)
            : GenerateFixedSchedule(amount, term, annualRate, startDate);
    }

    public List<PaymentScheduleResponse> GenerateFixedSchedule(
        decimal amount,
        int term,
        decimal annualRate,
        DateTime startDate)
    {
        var monthlyRate = (decimal)Math.Pow((double)(1 + annualRate / 100), 1.0 / 12.0) - 1;
        var monthlyPayment = CalculateMonthlyPayment(amount, term, annualRate);

        var balance = amount;
        var schedule = new List<PaymentScheduleResponse>();

        for (var i = 1; i <= term; i++)
        {
            var interest = Math.Round(balance * monthlyRate, 2);
            var principal = Math.Round(monthlyPayment - interest, 2);
            balance = Math.Round(balance - principal, 2);

            if (i == term && balance != 0)
            {
                principal += balance;
                balance = 0;
            }

            schedule.Add(new PaymentScheduleResponse
            {
                PaymentNumber = i,
                DueDate = AddMonthsKeepingDay(startDate, i),
                TotalPayment = monthlyPayment,
                Principal = principal,
                Interest = interest,
                RemainingBalance = balance,
                Status = PaymentScheduleStatus.Pending
            });
        }

        return schedule;
    }

    public List<PaymentScheduleResponse> GenerateDecreasingSchedule(
        decimal amount,
        int term,
        decimal annualRate,
        DateTime startDate)
    {
        var monthlyRate = (decimal)Math.Pow((double)(1 + annualRate / 100), 1.0 / 12.0) - 1;
        var fixedPrincipal = Math.Round(amount / term, 2);

        var balance = amount;
        var schedule = new List<PaymentScheduleResponse>();

        for (var i = 1; i <= term; i++)
        {
            var interest = Math.Round(balance * monthlyRate, 2);
            var principal = i == term ? balance : Math.Min(fixedPrincipal, balance);
            var totalPayment = Math.Round(principal + interest, 2);
            balance = Math.Round(balance - principal, 2);

            schedule.Add(new PaymentScheduleResponse
            {
                PaymentNumber = i,
                DueDate = AddMonthsKeepingDay(startDate, i),
                TotalPayment = totalPayment,
                Principal = principal,
                Interest = interest,
                RemainingBalance = balance,
                Status = PaymentScheduleStatus.Pending
            });
        }

        return schedule;
    }

    private decimal CalculateFirstDecreasingPayment(decimal amount, int term, decimal annualRate)
    {
        var monthlyRate = (decimal)Math.Pow((double)(1 + annualRate / 100), 1.0 / 12.0) - 1;
        var fixedPrincipal = Math.Round(amount / term, 2);

        return Math.Round(fixedPrincipal + amount * monthlyRate, 2);
    }

    private DateTime AddMonthsKeepingDay(DateTime date, int months)
    {
        var target = date.AddMonths(months);
        var day = Math.Min(date.Day, DateTime.DaysInMonth(target.Year, target.Month));

        return new DateTime(target.Year, target.Month, day, 0, 0, 0, DateTimeKind.Utc);
    }
}
