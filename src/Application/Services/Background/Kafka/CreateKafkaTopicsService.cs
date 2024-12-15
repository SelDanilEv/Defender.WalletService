using Defender.Kafka.BackgroundServices;
using Defender.Kafka.Configuration.Options;
using Defender.Kafka.Service;
using Defender.WalletService.Common.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Defender.WalletService.Application.Services.Background.Kafka;

public class CreateKafkaTopicsService(
    IKafkaTopicNameResolver kafkaTopicNameResolver,
    IOptions<KafkaOptions> kafkaOptions,
    ILogger<CreateKafkaTopicsService> logger)
    : EnsureTopicsCreatedService(kafkaOptions, logger)
{
    protected override IEnumerable<string> Topics =>
        [
            kafkaTopicNameResolver.ResolveTopicName(KafkaTopic.ScheduledTasks.GetName()),
            kafkaTopicNameResolver.ResolveTopicName(KafkaTopic.TransactionsToProcess.GetName()),
        ];

    protected override short ReplicationFactor => 1;

    protected override int NumPartitions => 3;
}
