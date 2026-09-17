using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace CreditOps.Function.HttpClients.Implementations;

public class UpdateLoanApiClient : IUpdateLoanApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UpdateLoanApiClient> _logger;

    public UpdateLoanApiClient(HttpClient httpClient, ILogger<UpdateLoanApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task<UpdateDecisionResponse?> UpdateDecisionAsync(Guid id, UpdateDecisionRequest request, CancellationToken ct = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Correlation-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", request.CorrelationId);

            var httpResponse = await _httpClient.PostAsJsonAsync($"api/LoanApplications/{id}/decision", request, ct);

            httpResponse.EnsureSuccessStatusCode();

            return await httpResponse.Content.ReadFromJsonAsync<UpdateDecisionResponse>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking LoanApplication api, ApplicationId = {ApplicationId}",
                request.ApplicationId);
            throw;
        }
    }
}