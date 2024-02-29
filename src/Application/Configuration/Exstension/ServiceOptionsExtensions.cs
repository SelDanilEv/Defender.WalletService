using Defender.Common.Configuration.Options;
using Defender.Mongo.MessageBroker.Extensions;
using Defender.WalletService.Application.Configuration.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Defender.WalletService.Application.Configuration.Exstension;

public static class ServiceOptionsExtensions
{
    public static IServiceCollection AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceOptions>(configuration.GetSection(nameof(ServiceOptions)));

        services.AddMongoMessageBrokerServices(opt =>
        {
            var mongoDbOptions = services
                .BuildServiceProvider()
                .GetRequiredService<IOptions<MongoDbOptions>>()
                .Value;

            opt.MongoDbConnectionString = mongoDbOptions.ConnectionString;
            opt.MongoDbDatabaseName = mongoDbOptions.GetDatabaseName();
        });

        return services;
    }
}