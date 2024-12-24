using Defender.Kafka.Default;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Common.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Defender.WalletService.Application.Services.Background.Kafka;

public class EventListenerService(
    IDefaultKafkaConsumer<string> kafkaStringEventConsumer,
    IDefaultKafkaConsumer<string> transactionsToProceedConsumer,
    ITransactionProcessingService transactionProcessingService,
    ILogger<EventListenerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(10_000, stoppingToken);

        await Task.WhenAll(
            kafkaStringEventConsumer.StartConsuming(
                KafkaTopic.ScheduledTasks.GetName(),
                ConsumerGroup.Primary.GetName(),
                HandleStringEvent,
                stoppingToken),
            transactionsToProceedConsumer.StartConsuming(
                KafkaTopic.TransactionsToProcess.GetName(),
                ConsumerGroup.Primary.GetName(),
                transactionProcessingService.ProcessTransaction,
                stoppingToken)
        );
    }

    private Task HandleStringEvent(string @event)
    {
        switch (@event.ToEvent())
        {
            // case KafkaEvent.:
            //     _ = lotteryProcessingService.QueueLotteriesForProcessing();
            //     break;
            default:
                logger.LogWarning("Unknown event: {0}", @event);
                break;
        }

        return Task.CompletedTask;
    }
}