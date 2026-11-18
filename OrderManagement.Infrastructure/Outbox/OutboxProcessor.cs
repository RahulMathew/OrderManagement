using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Abstractions.Persistence;
using OrderManagement.Infrastructure.Messaging.Kafka.Abstractions;
using OrderManagement.Infrastructure.Persistence.Repositories.Abstractions;

namespace OrderManagement.Infrastructure.Outbox
{
    public sealed class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessor> _logger;
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

        private const int BatchSize = 50;

        public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox processor started.");

            using var timer = new PeriodicTimer(PollingInterval);

            try
            {
                await ProcessMessagesAsync(stoppingToken);

                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await ProcessMessagesAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown
            }
        }

        private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var kafkaPublisher = scope.ServiceProvider.GetRequiredService<IKafkaPublisher>();

                var messages = await outboxRepository.GetPendingAsync(BatchSize, cancellationToken);

                foreach (var message in messages)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        // Step 1: Publish to Kafka
                        await kafkaPublisher.PublishAsync(message.Id,
                            message.EventType,
                            message.Payload,
                            cancellationToken);

                        // Step 2: Mark message as processed
                        var result = await outboxRepository.MarkProcessedAsync(message.Id, cancellationToken);

                        if (!result.IsSuccess)
                        {
                            _logger.LogWarning("Unable to mark Outbox message {MessageId} as processed. {Error}",
                                message.Id,
                                result.Error?.Message);

                            continue;
                        }

                        // Step 3: Commit through UnitOfWork
                        await unitOfWork.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("Outbox message {MessageId} processed.", message.Id);
                    }
                    catch (OperationCanceledException)
                        when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,"Failed to process outbox message {MessageId}.", message.Id);
                    }
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox processing cycle failed.");
            }
        }
    }
}
