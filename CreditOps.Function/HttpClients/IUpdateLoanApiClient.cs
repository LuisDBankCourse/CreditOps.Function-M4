namespace CreditOps.Function.HttpClients;

public interface IUpdateLoanApiClient
{
    Task<UpdateDecisionResponse?> UpdateDecisionAsync(Guid id, UpdateDecisionRequest request, CancellationToken ct = default);
}

public record UpdateDecisionRequest
(
    Guid ApplicationId,
    int Score,
    string RiskLevel,
    string Decision,
    string Reason,
    string CorrelationId

);

public record UpdateDecisionResponse
(
    Guid ApplicationId,
    int Status,
    string Message
);