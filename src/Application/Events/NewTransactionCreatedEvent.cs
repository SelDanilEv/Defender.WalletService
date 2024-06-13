using Defender.Mongo.MessageBroker.Models.QueueMessage;

namespace Defender.WalletService.Application.Events;

public record NewTransactionCreatedEvent : BaseQueueMessage
{
    public string? TransactionId { get; set; }
}
