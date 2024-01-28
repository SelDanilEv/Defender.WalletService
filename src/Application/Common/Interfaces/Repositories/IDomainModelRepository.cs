using Defender.WalletService.Domain.Entities;

namespace Defender.WalletService.Application.Common.Interfaces.Repositories;

public interface IDomainModelRepository
{
    Task<DomainModel> GetDomainModelByIdAsync(Guid id);
}
