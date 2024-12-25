using Defender.DistributedCache;
using Defender.Kafka;
using Defender.Kafka.Default;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Common.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Defender.WalletService.Application.Services.Background.Kafka;

public class EventListenerService(
    IDefaultKafkaConsumer<string> kafkaStringEventConsumer,
    IDefaultKafkaConsumer<string> transactionsToProceedConsumer,
    IPostgresCacheCleanupService cacheCleanupService,
    ITransactionProcessingService transactionProcessingService,
    ILogger<EventListenerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(10_000, stoppingToken);

        await Task.WhenAll(
            kafkaStringEventConsumer.StartConsuming(
                Topic.DistributedCache.GetName(),
                ConsumerGroup.DistributedCacheMember.GetName(),
                HandleStringEvent,
                stoppingToken),
            transactionsToProceedConsumer.StartConsuming(
                KafkaTopic.TransactionsToProcess.GetName(),
                ConsumerGroup.Primary.GetName(),
                transactionProcessingService.ProcessTransaction,
                stoppingToken)
        );
    }

    private async Task HandleStringEvent(string @event)
    {
        try
        {
            logger.LogInformation("Incoming event: {Event}", @event);

            switch (@event.ToEvent())
            {
                case KafkaEvent.StartCacheCleanup:
                    await cacheCleanupService.CheckAndRunCleanupAsync();
                    break;
                default:
                    logger.LogWarning("Unknown event: {0}", @event);
                    break;
            }
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error while handling event: {Event}", @event);
        }
    }
}