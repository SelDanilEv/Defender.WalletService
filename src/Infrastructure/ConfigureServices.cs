using System.Reflection;
using Defender.Common.Configuration.Options;
using Defender.Mongo.MessageBroker.Extensions;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Infrastructure.Common.Interfaces;
using Defender.WalletService.Infrastructure.Repositories;
using Defender.WalletService.Infrastructure.Repositories.DomainModels;
using Defender.WalletService.Infrastructure.Services;
using Defender.WalletService.Infrastructure.Services.Background;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Defender.WalletService.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services
            .RegisterServices()
            .RegisterRepositories()
            .RegisterApiClients()
            .RegisterClientWrappers();

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

    private static IServiceCollection RegisterClientWrappers(this IServiceCollection services)
    {
        //services.AddTransient<IServiceWrapper, ServiceWrapper>();

        return services;
    }

    private static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddTransient<IWalletManagementService, WalletManagementService>();
        services.AddTransient<ITransactionManagementService, TransactionManagementService>();
        services.AddTransient<ITransactionProcessingService, TransactionProcessingService>();

        services.AddHostedService<TransactionConsumerService>();

        return services;
    }

    private static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        services.AddSingleton<ITransactionRepository, TransactionRepository>();
        services.AddSingleton<IWalletRepository, WalletRepository>();

        return services;
    }

    private static IServiceCollection RegisterApiClients(
        this IServiceCollection services)
    {
        //services.RegisterIdentityClient(
        //    (serviceProvider, client) =>
        //    {
        //        client.BaseAddress = new Uri(serviceProvider.GetRequiredService<IOptions<ServiceOptions>>().Value.Url);
        //    });

        return services;
    }

}
