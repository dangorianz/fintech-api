namespace fintech_api.Models;

public enum LoanType
{
    Fixed = 1,
    Decreasing = 2
}

public enum LoanStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Active = 4
}

public enum PaymentScheduleStatus
{
    Pending = 1,
    Paid = 2
}

public enum TransactionType
{
    Disbursement = 1,
    Payment = 2,
    Transfer = 3
}

public enum TransactionStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3
}