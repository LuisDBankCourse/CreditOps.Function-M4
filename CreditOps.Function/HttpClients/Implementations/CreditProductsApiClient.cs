using CreditOps.Function.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CreditOps.Function.HttpClients.Implementations;

public class CreditProductsApiClient : ICreditProductsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CreditProductsApiClient> _logger;

    public CreditProductsApiClient(HttpClient httpClient, ILogger<CreditProductsApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task<CreditProductApiResponse?> GetCreditProductAsync(Guid creditProductId, string correlationId, CancellationToken ct = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Correlation-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);

            var response = await _httpClient.GetFromJsonAsync<CreditProductApiResponse>($"/api/CreditProducts/{creditProductId}", ct);

            return response;

        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Product not found, creditProductId = {creditProductId}, correlationId={correlationId}",
                creditProductId, correlationId);


            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking creditProducts api, creditProductId = {creditProductId}, correlationId={correlationId}",
                creditProductId, correlationId);
            throw;
        }
    }
}