using Defender.WalletService.Domain.Entities.Wallets;
using MongoDB.Driver;

namespace Defender.WalletService.Application.Common.Interfaces.Repositories;

public interface IWalletRepository
{
    Task<Wallet> GetWalletByUserIdAsync(Guid userId);

    Task<Wallet> GetWalletByNumberAsync(int walletNumber);

    Task<Wallet> CreateNewWalletAsync(Wallet wallet);

    Task<Wallet> UpdateCurrencyAccountsAsync(
        Guid walletId,
        HashSet<CurrencyAccount> currencyAccounts,
        IClientSessionHandle? clientSessionHandle = null);

    Task<IClientSessionHandle> OpenSessionAsync();
}
