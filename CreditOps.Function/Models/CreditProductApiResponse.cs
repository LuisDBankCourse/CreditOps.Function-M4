namespace CreditOps.Function.Models;

public record CreditProductApiResponse
(
    Guid Id,
    string Code,
    string Name,
    decimal MinAmount,
    decimal MaxAmount,
    int MinTermMonths,
    int MaxTermMonths,
    decimal MaxInstallmentIncomeRatio,
    bool IsActive

);