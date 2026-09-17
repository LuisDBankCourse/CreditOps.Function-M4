using CreditOps.Function.HttpClients;
using CreditOps.Function.Models;
using Microsoft.Extensions.Logging;

namespace CreditOps.Function.Service
{
    public class CreditOrchestratorService
    {
        private readonly ILogger<CreditOrchestratorService> _logger;
        private readonly ICustomerApiClient _customerApi;
        private readonly ICreditProductsApiClient _creditProductsApiClient;
        private readonly IRiskScoringApiClient _riskScoringApi;
        private readonly IUpdateLoanApiClient _loanApiClient;
        public CreditOrchestratorService(ILogger<CreditOrchestratorService> logger,
            ICustomerApiClient customerApi,
            ICreditProductsApiClient creditProductsApi,
            IRiskScoringApiClient riskScoringApi,
            IUpdateLoanApiClient loanApiClient
        )
        {
            _logger = logger;
            _customerApi = customerApi;
            _creditProductsApiClient = creditProductsApi;
            _riskScoringApi = riskScoringApi;
            _loanApiClient = loanApiClient;
        }

        public async Task ProcessAsync(LoanApplicationCreatedEvent @event, CancellationToken ct = default)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = @event.CorrelationId,
                ["ApplicationId"] = @event.ApplicationId,
                ["CustomerId"] = @event.CustomerId,
                ["ProductId"] = @event.ProductId

            });

            _logger.LogInformation("Staring orchestrator process Amount = {Amount},Term = {Term}", @event.RequestedAmount, @event.TermMonths);

            //Step 1: Get customer

            var customer = await _customerApi.GetCustomerAsync(@event.CustomerId, @event.CorrelationId, ct);

            if (customer is null)
            {
                _logger.LogWarning("customer not founded CustomerId = {CustomerId},CorrelationId = {CorrelationId}", @event.CustomerId, @event.CorrelationId);
                
                //UpdateDecision
                await UpdateDecisionAsync(
                    @event.ApplicationId,
                    score: 0,
                    riskLevel: "High",
                    decision: "Failed",
                    reason: $"Client with id {@event.CustomerId} doesn't exist",
                    @event.CorrelationId, ct);

                return;
            }


            //Step 2: Get the credit product

            var product = await _creditProductsApiClient.GetCreditProductAsync(@event.ProductId, @event.CorrelationId, ct);

            if (product is null)
            {
                _logger.LogWarning("Product not founded ProductId = {ProductId},CorrelationId = {CorrelationId}", @event.ProductId, @event.CorrelationId);

                await UpdateDecisionAsync(
                    @event.ApplicationId,
                    score: 0,
                    riskLevel: "High",
                    decision: "Rejected",
                    reason: $"Product with the id {@event.ProductId} doesn't exist",
                    @event.CorrelationId, ct);

                NotifyAsync(@event.ApplicationId, customer.Email, "Rejected", "Product with the id {@event.ProductId} doesn't exist");

                return;
            }

            //Step 3: Conduct the assessment using riskcoring
            var riskRequest = new EvaluateRiskRequest
            (
                ApplicationId: @event.ApplicationId,
                CustomerId: @event.CustomerId,
                MonthlyIncome: customer.MonthlyIncome,
                EmploymentType: customer.EmploymentType,
                RequestedAmount: @event.RequestedAmount,
                TermMonths: @event.TermMonths,
                MinAmount: product.MinAmount,
                MaxAmount: product.MaxAmount,
                MinTermMonths: product.MinTermMonths,
                MaxTermMonths: product.MaxTermMonths,
                MaxInstallmentIncomeRatio: product.MaxInstallmentIncomeRatio,
                CorrelationId: @event.CorrelationId
            );

            var riskResult = await _riskScoringApi.EvaluateRiskAsync(riskRequest, ct);

            if (riskResult is null)
            {
                throw new InvalidOperationException("RiskScoring response null. The evaluation can't be proceed.");
            }

            _logger.LogInformation("Risk evaluation completed ApplicationId = {ApplicationId},Score = {Score},RiskLevel = {RiskLevel},Decision = {Decision}",
                @event.ApplicationId, riskResult.Score, riskResult.RiskLevel, riskResult.Decision
            );


            //Step 4: Notify to client

            _logger.LogInformation("Risk assessment completed ApplicationId = {ApplicationId},RequestedAmount = {RequestedAmount},CorrelationId = {CorrelationId}",
                @event.ApplicationId, @event.RequestedAmount, @event.CorrelationId
                );
            
            await UpdateDecisionAsync(
                @event.ApplicationId,
                riskResult.Score,
                riskResult.RiskLevel,
                riskResult.Decision,
                riskResult.Reason,
                @event.CorrelationId, ct);
            
            //Step 5: notify the client
            NotifyAsync(@event.ApplicationId, customer.Email, riskResult.Decision, riskResult.Reason);

            _logger.LogInformation("Credit Orchestrator completed ApplicationId = {ApplicationId},RequestedAmount = {RequestedAmount},CorrelationId = {CorrelationId}",
                @event.ApplicationId, @event.RequestedAmount, @event.CorrelationId
            );

        }
        
        private async Task UpdateDecisionAsync(
            Guid applicationId,
            int score,
            string riskLevel,
            string decision,
            string reason,
            string correlationId, CancellationToken ct
        )
        {
            var request = new UpdateDecisionRequest(applicationId, score, riskLevel, decision, reason, correlationId);

            var updateResult = _loanApiClient.UpdateDecisionAsync(applicationId, request, ct);

            if (updateResult is null)
            {
                throw new InvalidOperationException("LoanApplicarion api returned null. The evaluation can't be proceed.");
            }

            _logger.LogInformation("Desition updated. ApplicationId = {ApplicationId}, Status = {Status}",
                applicationId, updateResult.Status
            );

        }

        private void NotifyAsync(Guid applicationId, string email, string decision, string reason)
        {

            _logger.LogWarning("[SIMULATE NOTIFICATION]. ApplicationId = {ApplicationId}, email = {email},decision={decision},reason = {reason}",
                applicationId, email, decision, reason);
        }

    }
}