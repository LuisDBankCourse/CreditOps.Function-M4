namespace CreditOps.Function.Models;

public record CustomerApiResponse
(
    Guid Id,
    string FullName,
    string DocumentNumber,
    string Email,
    decimal MonthlyIncome,
    string EmploymentType

);