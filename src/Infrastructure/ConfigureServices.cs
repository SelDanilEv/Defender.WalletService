using System.Reflection;
using Defender.Common.Clients.Identity;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Application.Common.Interfaces.Wrapper;
using Defender.WalletService.Application.Configuration.Options;
using Defender.WalletService.Infrastructure.Clients.Service;
using Defender.WalletService.Infrastructure.Repositories.DomainModels;
using Defender.WalletService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Defender.WalletService.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services
            .RegisterServices()
            .RegisterRepositories()
            .RegisterApiClients(configuration)
            .RegisterClientWrappers();

        return services;
    }

    private static IServiceCollection RegisterClientWrappers(this IServiceCollection services)
    {
        services.AddTransient<IServiceWrapper, ServiceWrapper>();

        return services;
    }

    private static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddTransient<IService, Service>();

        return services;
    }

    private static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IDomainModelRepository, DomainModelRepository>();

        return services;
    }

    private static IServiceCollection RegisterApiClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterIdentityClient(
            (serviceProvider, client) =>
            {
                client.BaseAddress = new Uri(serviceProvider.GetRequiredService<IOptions<ServiceOptions>>().Value.Url);
            });

        return services;
    }

}
