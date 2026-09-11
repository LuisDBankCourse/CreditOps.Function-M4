using CreditOps.Function.Models;

namespace CreditOps.Function.HttpClients;

public interface ICustomerApiClient
{
    Task<CustomerApiResponse?> GetCustomerAsync(Guid customerId, string correlationId, CancellationToken ct = default);
}