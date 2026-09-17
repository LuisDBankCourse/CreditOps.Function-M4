using CreditOps.Function.Models;

namespace CreditOps.Function.HttpClients;

public interface IRiskScoringApiClient
{
    Task<EvaluateRiskResponse?> EvaluateRiskAsync(EvaluateRiskRequest request, CancellationToken ct = default);
}

public record EvaluateRiskRequest
(

    Guid ApplicationId,
    Guid CustomerId,
    decimal MonthlyIncome,
    string EmploymentType,
    decimal RequestedAmount,
    int TermMonths,
    decimal MinAmount,
    decimal MaxAmount,
    int MinTermMonths,
    int MaxTermMonths,
    decimal MaxInstallmentIncomeRatio,
    string CorrelationId

);

public record EvaluateRiskResponse
(

    int Score,
    string RiskLevel,
    string Decision,
    string Reason,
    Dictionary<string, int> RuleBreakdown

);