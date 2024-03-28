using Defender.Common.Mapping;
using Defender.WalletService.Application.DTOs;
using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Entities.Wallets;

namespace Defender.WalletService.Application.Common.Mappings;

public class MappingProfile : BaseMappingProfile
{
    public MappingProfile()
    {
        CreateMap<Wallet, WalletDto>()
            .ForMember(
                dto => dto.OwnerId,
                opt => opt.MapFrom(src => src.Id));

        CreateMap<Wallet, PublicWalletInfoDto>()
            .ForMember(
                dto => dto.OwnerId,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(
                dto => dto.Currencies,
                opt => opt.MapFrom(src => src.CurrencyAccounts.Select(x => x.Currency)));

        CreateMap<Transaction, TransactionDto>();
    }
}
