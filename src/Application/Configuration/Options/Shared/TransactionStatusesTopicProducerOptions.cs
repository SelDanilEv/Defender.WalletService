using Defender.Common.DB.SharedStorage.Options.Base;
using Defender.Common.Enums;
using Defender.Common.Helpers;

namespace Defender.WalletService.Application.Configuration.Options.Shared;

public record TransactionStatusesTopicProducerOptions
    : TransactionStatusesTopicBaseOptions
{
    public TransactionStatusesTopicProducerOptions(AppEnvironment envPrefix)
        : base(envPrefix)
    {
        MongoDbConnectionString = SecretsHelper.GetSecretSync(
            Secret.SharedAdminConnectionString, true);
    }
}
