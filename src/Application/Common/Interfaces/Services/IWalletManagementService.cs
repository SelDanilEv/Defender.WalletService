using Defender.WalletService.Domain.Entities.Wallets;
using Defender.WalletService.Domain.Enums;

namespace Defender.WalletService.Application.Common.Interfaces;

public interface IWalletManagementService
{
    Task<Wallet> GetWalletByUserIdAsync(Guid userId);
    Task<Wallet> GetWalletByNumberAsync(int walletNumber);
    Task<Wallet> CreateNewWalletAsync();
    Task<Wallet> AddCurrencyAccountAsync(
        Guid userId,
        Currency currency,
        bool isDefault = false);

    Task<Wallet> SetDefaultCurrencyAccountAsync(
        Guid userId,
        Currency currency);
}
