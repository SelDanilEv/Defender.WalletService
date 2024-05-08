namespace Defender.WalletService.Infrastructure.Consts;

internal class MessageBroker
{
    public record Queues
    {
        public const string TransactionEventQueue = "incoming-transactions";
    }

    public record MessageTypes
    {
        public const string Transaction = "Transaction";
    }
}
