namespace Defender.WalletService.Infrastructure.Consts;

internal class MessageBroker
{
    public record Topics
    {
        public const string TransactionTopic = "incoming-transactions";
    }

    public record MessageTypes
    {
        public const string Transaction = "Transaction";
    }
}
