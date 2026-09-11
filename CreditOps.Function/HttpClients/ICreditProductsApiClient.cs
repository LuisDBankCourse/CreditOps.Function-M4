using CreditOps.Function.Models;

namespace CreditOps.Function.HttpClients;

public interface ICreditProductsApiClient
{
    Task<CreditProductApiResponse?> GetCreditProductAsync(Guid productId, string correlationId, CancellationToken ct = default);

}