using Defender.Common.Entities;

namespace Defender.WalletService.Domain.Entities.Wallets;

public sealed class Wallet : BaseWallet, IBaseModel
{
    public Guid Id { get; set; }
}
