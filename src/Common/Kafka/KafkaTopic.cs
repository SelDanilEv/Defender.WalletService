namespace Defender.WalletService.Common.Kafka;

public enum KafkaTopic
{
    ScheduledTasks,
    TransactionsToProcess
}

public static class KafkaTopicExtensions
{
    private const string ServiceName = "WalletService";

    private static readonly Dictionary<KafkaTopic, string> TopicToStringMap =
        new()
        {
            { KafkaTopic.ScheduledTasks, $"{ServiceName}_scheduled-tasks-topic" },
            { KafkaTopic.TransactionsToProcess, $"{ServiceName}_transactions-to-process-topic" },
        };


    public static string GetName(this KafkaTopic topic)
    {
        if (TopicToStringMap.TryGetValue(topic, out var name))
        {
            return name;
        }
        throw new ArgumentException($"Unknown topic: {topic}");
    }
}