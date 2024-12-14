using Defender.Common.Configuration.Options.Kafka;
using Defender.Common.Kafka.BackgroundServices;
using Defender.Common.Kafka.Service;
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
