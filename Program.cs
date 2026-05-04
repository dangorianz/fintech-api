using fintech_api.Data;
using fintech_api.Repositories.Implementations;
using fintech_api.Repositories.Interfaces;
using fintech_api.Services;
using fintech_api.Services.Interfaces;
using fintech_api.Utils;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrWhiteSpace(databaseUrl))
{
    throw new Exception("DATABASE_URL is required.");
}

var connectionString = ConvertDatabaseUrlToConnectionString(databaseUrl);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<FinancialCalculator>();

builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

static string ConvertDatabaseUrlToConnectionString(string databaseUrl)
{
    var uri = new Uri(databaseUrl);

    var userInfo = uri.UserInfo.Split(':');

    var username = userInfo[0];
    var password = userInfo[1];

    var host = uri.Host;
    var port = uri.Port;
    var database = uri.AbsolutePath.TrimStart('/');

    // If connecting to localhost (development), disable SSL to avoid server SSL configuration issues.
    var sslModePart = (host == "localhost" || host == "127.0.0.1")
        ? "SSL Mode=Disable"
        : "SSL Mode=Require;Trust Server Certificate=true";

    return $"Host={host};Port={port};Database={database};Username={username};Password={password};{sslModePart}";
}
