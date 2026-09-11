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
        public CreditOrchestratorService(ILogger<CreditOrchestratorService> logger, ICustomerApiClient customerApi, ICreditProductsApiClient creditProductsApi)
        {
            _logger = logger;
            _customerApi = customerApi;
            _creditProductsApiClient = creditProductsApi;
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

                return;
            }


            //Step 2: Get the credit product

            var product = await _creditProductsApiClient.GetCreditProductAsync(@event.ProductId, @event.CorrelationId, ct);

            if (product is null)
            {
                _logger.LogWarning("Product not founded ProductId = {ProductId},CorrelationId = {CorrelationId}", @event.ProductId, @event.CorrelationId);

                //UpdateDecision

                return;
            }

            //Step 3: Conduct the assessment using riskcoring


            //Step 4: Notify to client

            _logger.LogInformation("Risk assessment completed ApplicationId = {ApplicationId},RequestedAmount = {RequestedAmount},CorrelationId = {CorrelationId}",
                @event.ApplicationId, @event.RequestedAmount, @event.CorrelationId
                );

        }
    }
}