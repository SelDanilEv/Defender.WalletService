using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Defender.WalletService.Infrastructure.Common.Interfaces;
using Defender.WalletService.Infrastructure.Models;
using Microsoft.Extensions.Hosting;
using static Defender.WalletService.Infrastructure.Consts.MessageBroker;

namespace Defender.WalletService.Infrastructure.Services.Background;

public class TransactionConsumerService : BackgroundService
{
    private readonly IQueueConsumer _consumer;
    private readonly ITransactionProcessingService _transactionProcessingService;

    public TransactionConsumerService(
        IQueueConsumer consumer,
        ITransactionProcessingService transactionProcessingService)
    {
        _transactionProcessingService = transactionProcessingService;

        _consumer = consumer;

        _consumer.SetQueue(Queues.TransactionEventQueue)
            .SetMessageType(MessageTypes.Transaction);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.SubscribeQueueAsync<TransactionEvent>(
            async (transaction) =>
                await _transactionProcessingService.ProcessTransaction(transaction),
            stoppingToken);
    }
}
