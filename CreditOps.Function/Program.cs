using Azure.Monitor.OpenTelemetry.Exporter;
using CreditOps.Function.HttpClients;
using CreditOps.Function.HttpClients.Implementations;
using CreditOps.Function.Service;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{


    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();

   
}


builder.Services.AddHttpClient<ICustomerApiClient, CustomerApiClient>(x =>
    x.BaseAddress = new Uri(builder.Configuration["Service:CustomerApi"]
                            ?? throw new InvalidOperationException("Service:CustomerApi not configured")));

builder.Services.AddHttpClient<ICreditProductsApiClient, CreditProductsApiClient>(x =>
    x.BaseAddress = new Uri(builder.Configuration["Service:CreditProductsApi"]
                            ?? throw new InvalidOperationException("Service:CreditProductsApi not configured")));

builder.Services.AddHttpClient<IRiskScoringApiClient, RiskScoringApiClient>(x =>
    x.BaseAddress = new Uri(builder.Configuration["Service:RiskScoringApi"]
                            ?? throw new InvalidOperationException("Service:RiskScoring not configured")));

builder.Services.AddHttpClient<IUpdateLoanApiClient, UpdateLoanApiClient>(x =>
    x.BaseAddress = new Uri(builder.Configuration["Service:LoanApplicationApi"]
                            ?? throw new InvalidOperationException("Service:LoanApplicationApi not configured")));

builder.Services.AddScoped<CreditOrchestratorService>();
builder.Build().Run();