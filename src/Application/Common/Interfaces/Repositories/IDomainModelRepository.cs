using Defender.ServiceTemplate.Domain.Entities;

namespace Defender.ServiceTemplate.Application.Common.Interfaces.Repositories;

public interface IDomainModelRepository
{
    Task<DomainModel> GetDomainModelByIdAsync(Guid id);
}
