using System.Reflection;
using Defender.Common.Enums;
using Defender.Common.Helpers;
using Defender.DistributedCache.Configuration.Options;
using Defender.DistributedCache.Postgres.Extensions;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Application.Services;
using Defender.WalletService.Application.Services.Background;
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

        services.AddCache(configuration);
        services.RegisterServices();

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

        services.AddHostedService<TransactionEventConsumerService>();

        return services;
    }
}
