using Azure.Messaging.ServiceBus;
using CreditOps.Function.Models;
using CreditOps.Function.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CreditOps.Function;

public class ProcessLoanApplicationFunction
{
    private readonly ILogger<ProcessLoanApplicationFunction> _logger;
    private readonly CreditOrchestratorService _orchestratorService;
    private const int MaxDeliveryCountBeforeDeadtLetter = 5;
    public ProcessLoanApplicationFunction(ILogger<ProcessLoanApplicationFunction> logger, CreditOrchestratorService orchestratorService)
    {
        _logger = logger;
        _orchestratorService = orchestratorService;
    }

    [Function(nameof(ProcessLoanApplicationFunction))]
    public async Task Run([ServiceBusTrigger("%LoanApplicationQueueName%", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions, CancellationToken ct)
    {
        //_logger.LogInformation("C# Queue trigger function processed: {messageText}", message.MessageText);

        _logger.LogInformation("The evaluation process has been triggered  MessageLength = {MessageLength}", message.Body.Length);
        LoanApplicationCreatedEvent? @event;
        try
        {
            @event = JsonSerializer.Deserialize<LoanApplicationCreatedEvent>(message.Body.ToString(), 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON, message sent to deat-letter. MessageId = {MessageId}, SequenceNumber={SequenceNumber}", message.MessageId, message.SequenceNumber);

            await messageActions.DeadLetterMessageAsync(message,
                deadLetterReason: "Invalid Json",
                deadLetterErrorDescription: ex.Message,
                cancellationToken: ct);
            return;
        }

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = @event.CorrelationId,
            ["ApplicationId"] = @event.ApplicationId
        });

        try
        {
            await _orchestratorService.ProcessAsync(@event, ct);

            await messageActions.CompleteMessageAsync(message, ct);
        }
        catch (Exception ex)
        {
            if (message.DeliveryCount >= MaxDeliveryCountBeforeDeadtLetter)
            {
                _logger.LogError(ex, "An error occurred during processing and it is being sent to deat-letter. MessageId = {MessageId}, SequenceNumber={SequenceNumber}", message.MessageId, message.SequenceNumber);


                await messageActions.DeadLetterMessageAsync(
                    message,
                deadLetterReason: "Error en el ProcessAsync",
                deadLetterErrorDescription: ex.Message,
                cancellationToken: ct);
            }
            else
            {
                _logger.LogError(ex, "An error occurred during processing and it is being sent to AbandonMessageAsync. MessageId = {MessageId}, SequenceNumber={SequenceNumber}", message.MessageId, message.SequenceNumber);

                await messageActions.AbandonMessageAsync(message, cancellationToken: ct);
            }

        }
    }
}
