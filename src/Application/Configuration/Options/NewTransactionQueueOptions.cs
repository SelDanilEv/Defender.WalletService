using Defender.Common.Configuration.Options;
using Defender.Mongo.MessageBroker.Configuration;
using Defender.WalletService.Application.Events;

namespace Defender.WalletService.Application.Configuration.Options;

public record NewTransactionQueueOptions
    : MessageBrokerOptions<NewTransactionCreatedEvent>
{
    public NewTransactionQueueOptions(MongoDbOptions mongoDbOptions)
    {
        MongoDbConnectionString = mongoDbOptions.ConnectionString;
        MongoDbDatabaseName = mongoDbOptions.GetDatabaseName();

        Name = "incoming-transactions";
        Type = "NewTransaction";

        MaxDocuments = 5000;
        MaxByteSize = int.MaxValue;
    }
}
