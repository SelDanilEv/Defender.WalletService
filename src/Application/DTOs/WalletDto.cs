using Defender.WalletService.Domain.Entities.Wallets;

namespace Defender.WalletService.Application.DTOs;

public class WalletDto : BaseWallet
{
    public Guid OwnerId { get; set; }
}
