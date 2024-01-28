using Defender.ServiceTemplate.Application.Common.Interfaces;
using Defender.ServiceTemplate.Application.Common.Interfaces.Repositories;

namespace Defender.ServiceTemplate.Infrastructure.Services;

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
