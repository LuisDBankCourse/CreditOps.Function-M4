using System.Net.Http.Json;
using CreditOps.Function.Models;
using Microsoft.Extensions.Logging;

namespace CreditOps.Function.HttpClients.Implementations;

public class RiskScoringApiClient : IRiskScoringApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RiskScoringApiClient> _logger;

    public RiskScoringApiClient(HttpClient httpClient, ILogger<RiskScoringApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task<EvaluateRiskResponse?> EvaluateRiskAsync(EvaluateRiskRequest request, CancellationToken ct = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Correlation-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", request.CorrelationId);

            var httpResponse = await _httpClient.PostAsJsonAsync("api/Risk/evaluate", request, ct);

            httpResponse.EnsureSuccessStatusCode();

            return await httpResponse.Content.ReadFromJsonAsync<EvaluateRiskResponse>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking riskscoring api, ApplicationId = {ApplicationId}",
                request.ApplicationId);
            throw;
        }
    }
}