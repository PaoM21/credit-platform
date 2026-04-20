using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using CreditService.Infrastructure.Persistence;
using CreditService.Infrastructure.Entities;

namespace CreditService.Infrastructure.Services
{
    public class OutboxPublisherService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _log;
        private readonly IConfiguration _config;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly string? _conn;
        private readonly string _topic;
        private readonly int _batchSize;
        private readonly TimeSpan _pollInterval;
            public OutboxPublisherService(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisherService> log, IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _log = log;
        _config = config;

        _conn = _config["ServiceBus:ConnectionString"];
        _topic = _config["ServiceBus:TopicName"] ?? "credit-events";
        _batchSize = int.Parse(_config["ServiceBus:BatchSize"] ?? "20");
        _pollInterval = TimeSpan.FromSeconds(int.Parse(_config["ServiceBus:PollIntervalSeconds"] ?? "5"));

        _retryPolicy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4)
            }, (ex, ts) => _log.LogWarning(ex, "Retrying publish after {Delay}", ts));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("OutboxPublisherService started");

        if (string.IsNullOrWhiteSpace(_conn))
        {
            _log.LogWarning("ServiceBus connection string not configured. OutboxPublisherService will not run.");
            return;
        }

        await using var client = new ServiceBusClient(_conn);
        var sender = client.CreateSender(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CreditDbContext>();

                var pending = await db.OutboxEvents
                    .Where(o => o.SentAt == null)
                    .OrderBy(o => o.CreatedAt)
                    .Take(_batchSize)
                    .ToListAsync(stoppingToken);

                if (!pending.Any())
                {
                    await Task.Delay(_pollInterval, stoppingToken);
                    continue;
                }

                foreach (var ev in pending)
                {
                    await _retryPolicy.ExecuteAsync(async () =>
                    {
                        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(ev.Payload))
                        {
                            MessageId = ev.MessageId
                        };
                        message.ApplicationProperties["Type"] = ev.Type;

                        await sender.SendMessageAsync(message, stoppingToken);

                        ev.SentAt = DateTime.UtcNow;
                        db.OutboxEvents.Update(ev);
                        await db.SaveChangesAsync(stoppingToken);

                        _log.LogInformation("Published outbox event {MessageId} type {Type}", ev.MessageId, ev.Type);
                    });
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error publishing outbox events, will retry after delay");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _log.LogInformation("OutboxPublisherService stopping");
    }
}
}
