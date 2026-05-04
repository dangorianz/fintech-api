using fintech_api.DTOs;
using fintech_api.Models;
using fintech_api.Repositories.Implementations;
using fintech_api.Repositories.Interfaces;
using fintech_api.Services;
using fintech_api.Utils;

namespace FinTech.Tests;

public sealed class LoanServiceTests
{
    [Fact]
    public void Fixed_payment_calculation_uses_french_system()
    {
        var simulation = FinancialCalculator.Simulate(
            amount: 10000m,
            term: 12,
            loanType: LoanType.Fixed,
            annualEffectiveRate: 0.24m,
            firstDueDate: new DateTime(2026, 5, 30, 0, 0, 0, DateTimeKind.Utc));

        Assert.Equal(945.60m, simulation.MonthlyPayment);
        Assert.Equal(12, simulation.Schedule.Count);
    }

    [Fact]
    public void Payment_schedule_has_expected_length_dates_and_zero_final_balance()
    {
        var simulation = FinancialCalculator.Simulate(
            amount: 5000m,
            term: 6,
            loanType: LoanType.Fixed,
            annualEffectiveRate: 0.24m,
            firstDueDate: new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc));

        Assert.Equal(6, simulation.Schedule.Count);
        Assert.Equal(new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc), simulation.Schedule[1].DueDate);
        Assert.Equal(0m, simulation.Schedule[^1].RemainingBalance);
    }

    [Theory]
    [InlineData(499)]
    [InlineData(50001)]
    public async Task Simulate_rejects_amount_outside_allowed_range(decimal amount)
    {
        var service = CreateLoanService();

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.SimulateAsync(new SimulateLoanRequest(amount, 12), CancellationToken.None));

        Assert.Equal("invalid_amount", exception.Code);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(61)]
    public async Task Simulate_rejects_term_outside_allowed_range(int term)
    {
        var service = CreateLoanService();

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.SimulateAsync(new SimulateLoanRequest(1000m, term), CancellationToken.None));

        Assert.Equal("invalid_term", exception.Code);
    }

    [Fact]
    public async Task Create_rejects_loan_when_debt_ratio_exceeds_40_percent()
    {
        var service = CreateLoanService();

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateAsync(new CreateLoanRequest("user-456", 50000m, 6), CancellationToken.None));

        Assert.Equal("debt_ratio_exceeded", exception.Code);
    }

    private static LoanService CreateLoanService()
    {
        var store = new InMemoryStore();
        ICustomerRepository customers = new InMemoryCustomerRepository(store);
        ILoanRepository loans = new InMemoryLoanRepository(store);
        ITransactionRepository transactions = new InMemoryTransactionRepository(store);
        return new LoanService(customers, loans, transactions);
    }
}
