using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Repositories;

namespace Defender.WalletService.Infrastructure.Services;

public class Service : IService
{
    private readonly IDomainModelRepository _accountInfoRepository;


    public Service(
        IDomainModelRepository accountInfoRepository)
    {
        _accountInfoRepository = accountInfoRepository;
    }

    public Task DoService()
    {
        throw new NotImplementedException();
    }
}
