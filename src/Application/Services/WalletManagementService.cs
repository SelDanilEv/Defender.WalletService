using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Domain.Entities.Wallets;
using Defender.WalletService.Domain.Enums;

namespace Defender.WalletService.Application.Services;

public class WalletManagementService(
        IWalletRepository walletRepository,
        ICurrentAccountAccessor currentAccountAccessor)
    : IWalletManagementService
{
    public async Task<Wallet> GetWalletByUserIdAsync(Guid userId)
    {
        return await walletRepository.GetWalletByUserIdAsync(userId);
    }

    public async Task<Wallet> GetWalletByNumberAsync(int walletNumber)
    {
        return await walletRepository.GetWalletByNumberAsync(walletNumber);
    }

    public async Task<Wallet> CreateNewWalletAsync(Guid? userId = null)
    {
        var wallet = new Wallet
        {
            Id = userId ?? currentAccountAccessor.GetAccountId(),
            CurrencyAccounts =
            [
                new CurrencyAccount
                {
                    Currency = Currency.USD,
                    Balance = 0,
                    IsDefault = true,
                }
            ]
        };

        return await walletRepository.CreateNewWalletAsync(wallet);
    }

    public async Task<Wallet> AddCurrencyAccountAsync(
        Guid userId,
        Currency currency,
        bool isDefault = false)
    {
        var wallet = await walletRepository.GetWalletByUserIdAsync(userId);

        if (wallet.CurrencyAccounts
            .FirstOrDefault(x => x.Currency == currency) != null)
        {
            throw new ServiceException(
                ErrorCode.BR_WLT_CurrencyAccountAlreadyExist);
        }

        if (isDefault)
        {
            wallet.CurrencyAccounts
                .ToList()
                .ForEach(x => x.IsDefault = false);
        }

        wallet.CurrencyAccounts.Add(new CurrencyAccount(currency, isDefault));

        return await walletRepository.UpdateCurrencyAccountsAsync(
            wallet.Id,
            wallet.CurrencyAccounts);
    }

    public async Task<Wallet> SetDefaultCurrencyAccountAsync(
        Guid userId,
        Currency currency)
    {
        var wallet = await walletRepository.GetWalletByUserIdAsync(userId);

        var newDefaultAccount = wallet.CurrencyAccounts
            .FirstOrDefault(x => x.Currency == currency);

        if (newDefaultAccount == null)
        {
            throw new ServiceException(
                ErrorCode.BR_WLT_CurrencyAccountIsNotExist);
        }

        newDefaultAccount.IsDefault = true;

        var oldDefaultAccount = wallet.CurrencyAccounts.FirstOrDefault(x => x.IsDefault);
        if (oldDefaultAccount != null)
            oldDefaultAccount.IsDefault = false;

        return await walletRepository.UpdateCurrencyAccountsAsync(
            wallet.Id,
            wallet.CurrencyAccounts);
    }

}
