using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Application.Services.Background;
using Defender.WalletService.Application.Services;

namespace Defender.WalletService.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.RegisterServices();

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
