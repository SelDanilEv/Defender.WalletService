using System.Reflection;
using Defender.Common.Enums;
using Defender.Common.Helpers;
using Defender.DistributedCache.Configuration.Options;
using Defender.DistributedCache.Postgres.Extensions;
using Defender.Kafka.Configuration.Options;
using Defender.Kafka.Extension;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Application.Services;
using Defender.WalletService.Application.Services.Background.Kafka;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Defender.WalletService.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.RegisterKafkaServices(configuration)
            .RegisterBackgroundServices()
            .AddCache(configuration)
            .RegisterServices();

        return services;
    }

    private static IServiceCollection RegisterKafkaServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddKafka(options =>
        {
            configuration.GetSection(nameof(KafkaOptions)).Bind(options);
        });

        return services;
    }

    private static IServiceCollection RegisterBackgroundServices(
        this IServiceCollection services)
    {
        services.AddHostedService<CreateKafkaTopicsService>();

        services.AddHostedService<EventListenerService>();

        return services;
    }

    private static IServiceCollection AddCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPostgresDistributedCache(options =>
        {
            options.ConnectionString = SecretsHelper.GetSecretSync(Secret.DistributedCacheConnectionString, true);
            configuration.GetSection(nameof(DistributedCacheOptions)).Bind(options);
        });

        return services;
    }

    private static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddTransient<IWalletManagementService, WalletManagementService>();
        services.AddTransient<ITransactionManagementService, TransactionManagementService>();
        services.AddTransient<ITransactionProcessingService, TransactionProcessingService>();

        return services;
    }
}
