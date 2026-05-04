using fintech_api.Models;
using Microsoft.EntityFrameworkCore;

namespace fintech_api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<PaymentSchedule> PaymentSchedules => Set<PaymentSchedule>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.InterestRate).HasPrecision(5, 2);
            entity.Property(x => x.MonthlyPayment).HasPrecision(18, 2);
            entity.Property(x => x.MonthlyIncome).HasPrecision(18, 2);

            entity.HasMany(x => x.PaymentSchedules)
                .WithOne(x => x.Loan)
                .HasForeignKey(x => x.LoanId);
        });

        modelBuilder.Entity<PaymentSchedule>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TotalPayment).HasPrecision(18, 2);
            entity.Property(x => x.Principal).HasPrecision(18, 2);
            entity.Property(x => x.Interest).HasPrecision(18, 2);
            entity.Property(x => x.RemainingBalance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();

            entity.Property(x => x.Amount).HasPrecision(18, 2);

            entity.HasOne(x => x.Loan)
                .WithMany()
                .HasForeignKey(x => x.LoanId)
                .IsRequired(false);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var loanId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        modelBuilder.Entity<Loan>().HasData(new Loan
        {
            Id = loanId,
            UserId = "user-1",
            Amount = 5000,
            Term = 12,
            InterestRate = 24,
            LoanType = LoanType.Fixed,
            Status = LoanStatus.Active,
            MonthlyPayment = 472.80m,
            MonthlyIncome = 3000,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}