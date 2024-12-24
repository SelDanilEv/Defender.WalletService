namespace Defender.WalletService.Common.Kafka;

public enum ConsumerGroup
{
    Primary,
}

public static class ConsumerGroupExtensions
{
    public static string GetName(this ConsumerGroup group)
    {
        return $"{AppConstants.ServiceName}_{group.ToString()}-group";
    }
}