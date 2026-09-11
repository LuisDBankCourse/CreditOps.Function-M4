namespace CreditOps.Function.Models;

public record LoanApplicationCreatedEvent
(
    Guid ApplicationId,
    Guid CustomerId,
    Guid ProductId,
    decimal RequestedAmount,
    int TermMonths,
    string Purpose,
    DateTime OcurredAtUtc,
    string CorrelationId
);