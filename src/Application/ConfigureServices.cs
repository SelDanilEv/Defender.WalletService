using System.Reflection;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Application.Services;
using Defender.WalletService.Application.Services.Background;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

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
