using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Defender.WalletService.Application.Services.Background;

public class TransactionEventConsumerService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var listeningTask = ListenForNewTransactions(stoppingToken);
        var retryingTask = RetryFailedTransactions(stoppingToken);

        await Task.WhenAll(listeningTask, retryingTask);
    }

    private async Task ListenForNewTransactions(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var consumer = CreateQueueConsumer(scope);
                var subscribeOption = CreateSubscribeOptions(scope);

                await consumer.SubscribeQueueAsync(subscribeOption, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break; // Expected during shutdown
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error subscribing to queue: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    private async Task RetryFailedTransactions(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var consumer = CreateQueueConsumer(scope);
                var subscribeOption = CreateSubscribeOptions(scope);

                await consumer.RetryMissedEventsAsync(subscribeOption, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break; // Expected during shutdown
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during transaction retry: {ex.Message}");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private static IQueueConsumer<NewTransactionCreatedEvent> CreateQueueConsumer(IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<IQueueConsumer<NewTransactionCreatedEvent>>();
    }

    private static SubscribeOptions<NewTransactionCreatedEvent> CreateSubscribeOptions(IServiceScope scope)
    {
        var transactionProcessingService = scope.ServiceProvider.GetRequiredService<ITransactionProcessingService>();

        return SubscribeOptionsBuilder<NewTransactionCreatedEvent>.Create()
                .SetAction(transactionProcessingService.ProcessTransaction)
                .Build();
    }
}
