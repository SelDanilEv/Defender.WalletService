using Defender.Kafka;
using Defender.Kafka.BackgroundServices;
using Defender.Kafka.Configuration.Options;
using Defender.Kafka.Service;
using Defender.WalletService.Common.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Defender.WalletService.Application.Services.Background.Kafka;

public class CreateKafkaTopicsService(
    IOptions<KafkaOptions> kafkaOptions,
    IKafkaEnvPrefixer kafkaEnvPrefixer,
    ILogger<CreateKafkaTopicsService> logger)
    : EnsureTopicsCreatedService(kafkaOptions,kafkaEnvPrefixer, logger)
{
    protected override IEnumerable<string> Topics =>
    [
        KafkaTopic.ScheduledTasks.GetName(),
        KafkaTopic.TransactionsToProcess.GetName(),
        Topic.TransactionStatusUpdates.GetName(),
    ];

    protected override short ReplicationFactor => 1;

    protected override int NumPartitions => 3;
}