using System.Net.Http.Json;
using CreditOps.Function.Models;
using Microsoft.Extensions.Logging;

namespace CreditOps.Function.HttpClients.Implementations;

public class CustomerApiClient : ICustomerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerApiClient> _logger;

    public CustomerApiClient(HttpClient httpClient, ILogger<CustomerApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task<CustomerApiResponse?> GetCustomerAsync(Guid customerId, string correlationId, CancellationToken ct = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Correlation-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);

            var response = await _httpClient.GetFromJsonAsync<CustomerApiResponse>($"/api/Customer/{customerId}", ct);

            return response;

        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Customer not found, customerId = {customerId}, correlationId={correlationId}",
                customerId, correlationId);


            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking customer api, customerId = {customerId}, correlationId={correlationId}",
                customerId, correlationId);
            throw;
        }
    }
}