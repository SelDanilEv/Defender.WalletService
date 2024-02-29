using Defender.Mongo.MessageBroker.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Domain.Entities.Transactions;
using Microsoft.Extensions.Hosting;
using static Defender.WalletService.Infrastructure.Consts.MessageBroker;

namespace Defender.WalletService.Infrastructure.Services.Background;

public class TransactionConsumerService : BackgroundService
{
    private readonly IConsumer _consumer;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionProcessingService _transactionProcessingService;

    public TransactionConsumerService(
        IConsumer consumer,
        ITransactionRepository transactionRepository,
        ITransactionProcessingService transactionProcessingService)
    {
        _transactionRepository = transactionRepository;
        _transactionProcessingService = transactionProcessingService;

        _consumer = consumer;

        _consumer.SetTopic(Topics.TransactionTopic).SetMessageType(MessageTypes.Transaction);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.SubscribeAsync<Transaction>(
            async (transaction) => await _transactionProcessingService.ProcessTransaction(transaction),
            async () =>
            {
                var lastRecord = await _transactionRepository.GetLastProccedTransaction();

                if (lastRecord == null)
                {
                    return DateTime.MinValue.AddMicroseconds(1);
                }

                return lastRecord.InsertedDateTime;
            },
            stoppingToken);
    }
}
