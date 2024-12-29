using System.Reflection;
using Defender.Common.Configuration.Options;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Defender.WalletService.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        var mongoDbOptions = services
            .BuildServiceProvider()
            .GetRequiredService<IOptions<MongoDbOptions>>()
            .Value;

        services
            .RegisterRepositories()
            .RegisterClientWrappers();


        return services;
    }

    private static IServiceCollection RegisterClientWrappers(this IServiceCollection services)
    {
        //services.AddTransient<IServiceWrapper, ServiceWrapper>();

        return services;
    }


    private static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        services.AddSingleton<ITransactionRepository, TransactionRepository>();
        services.AddSingleton<IWalletRepository, WalletRepository>();

        return services;
    }

}
