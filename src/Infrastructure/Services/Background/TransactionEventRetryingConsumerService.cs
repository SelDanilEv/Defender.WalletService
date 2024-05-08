using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Defender.WalletService.Infrastructure.Common.Interfaces;
using Defender.WalletService.Infrastructure.Models;
using Microsoft.Extensions.Hosting;
using static Defender.WalletService.Infrastructure.Consts.MessageBroker;

namespace Defender.WalletService.Infrastructure.Services.Background;

public class TransactionEventRetryingConsumerService : IHostedService, IDisposable
{
    private readonly IQueueConsumer _consumer;
    private readonly ITransactionProcessingService _transactionProcessingService;
    private Timer? _timer;
    private bool _isRunning = false;

    public TransactionEventRetryingConsumerService(
        IQueueConsumer consumer,
        ITransactionProcessingService transactionProcessingService)
    {
        _transactionProcessingService = transactionProcessingService;

        _consumer = consumer;

        _consumer.SetQueue(Queues.TransactionEventQueue)
            .SetMessageType(MessageTypes.Transaction);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(async _ => await Retry(null, cancellationToken),
            null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5));
        return Task.CompletedTask;
    }

    private async Task Retry(object? state, CancellationToken stoppingToken)
    {
        if (_isRunning)
        {
            return;
        }

        _isRunning = true;
        await _consumer.SubscribeQueueAsync<TransactionEvent>(
            async (transaction) =>
                await _transactionProcessingService.ProcessTransaction(transaction),
            stoppingToken); ;
        _isRunning = false;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
