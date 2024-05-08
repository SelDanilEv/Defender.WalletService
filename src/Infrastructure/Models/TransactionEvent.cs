using Defender.Mongo.MessageBroker.Models.QueueMessage;

namespace Defender.WalletService.Infrastructure.Models;
public class TransactionEvent : BaseQueueMessage
{
    public string? TransactionId { get; set; }
}
