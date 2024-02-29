using Defender.Common.Mapping;
using Defender.WalletService.Application.DTOs;
using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Entities.Wallets;

namespace Defender.WalletService.Application.Common.Mappings;

public class MappingProfile : BaseMappingProfile
{
    public MappingProfile()
    {
        CreateMap<Wallet, WalletDto>();
        CreateMap<Transaction, TransactionDto>();
    }
}
