using System; using System.Text; using System.Threading; using System.Threading.Tasks; using Azure.Messaging.ServiceBus; using Microsoft.Extensions.Hosting; using Microsoft.Extensions.Logging; using Microsoft.Extensions.Configuration; using Microsoft.Extensions.DependencyInjection; using Microsoft.EntityFrameworkCore; using Polly; using CreditService.Infrastructure.Persistence; using CreditService.Infrastructure.Entities; using AuditService.Api.Handlers;
namespace AuditService.Api.Services
{
    public class ServiceBusSubscriber : IHostedService, IAsyncDisposable
    {
        private readonly ILogger _log;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _provider;
        private ServiceBusClient? _client;
        private ServiceBusProcessor? _processor;
        private readonly int _maxRetries;
            public ServiceBusSubscriber(ILogger<ServiceBusSubscriber> log, IConfiguration config, IServiceProvider provider)
    {
        _log = log;
        _config = config;
        _provider = provider;
        _maxRetries = int.Parse(_config["ServiceBus:SubscriberMaxRetries"] ?? "5");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var conn = _config["ServiceBus:ConnectionString"];
        var topic = _config["ServiceBus:TopicName"] ?? "credit-events";
        var subscription = _config["ServiceBus:SubscriptionName"] ?? "audit-sub";

        _client = new ServiceBusClient(conn);
        var options = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = int.Parse(_config["ServiceBus:MaxConcurrentCalls"] ?? "5"),
            AutoCompleteMessages = false
        };

        _processor = _client.CreateProcessor(topic, subscription, options);
        _processor.ProcessMessageAsync += ProcessMessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        _log.LogInformation("Starting Audit subscriber {Topic}/{Subscription}", topic, subscription);
        return _processor.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor != null) await _processor.StopProcessingAsync(cancellationToken);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _log.LogError(args.Exception, "ServiceBus error: {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var messageId = message.MessageId;
        var body = Encoding.UTF8.GetString(message.Body);

        _log.LogInformation("Audit received {MessageId}", messageId);

        try
        {
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreditDbContext>();
            var handler = scope.ServiceProvider.GetRequiredService<CreditEventHandler>();

            // Idempotency
            var pm = new ProcessedMessage { Id = Guid.NewGuid(), MessageId = messageId, HandlerName = nameof(CreditEventHandler), ProcessedAt = DateTime.UtcNow };
            try
            {
                db.ProcessedMessages.Add(pm);
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _log.LogInformation("Message {MessageId} already processed", messageId);
                await args.CompleteMessageAsync(message);
                return;
            }

            // Business logic
            await handler.HandleAsync(body);

            await args.CompleteMessageAsync(message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Processing failed {MessageId}", messageId);
            var deliveryCount = message.DeliveryCount;
            if (deliveryCount > _maxRetries)
                await args.DeadLetterMessageAsync(message, "MaxRetriesExceeded", ex.Message);
            else
                await args.AbandonMessageAsync(message);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_processor != null) await _processor.DisposeAsync();
        if (_client != null) await _client.DisposeAsync();
    }
}
}
