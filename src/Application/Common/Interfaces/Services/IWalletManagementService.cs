﻿using Defender.WalletService.Domain.Entities.Wallets;
using Defender.WalletService.Domain.Enums;
using MongoDB.Driver;

namespace Defender.WalletService.Application.Common.Interfaces.Services;

public interface IWalletManagementService
{
    Task<IClientSessionHandle> OpenWalletUpdateSessionAsync();
    Task<Wallet> GetWalletByUserIdAsync(Guid userId);
    Task<Wallet> GetWalletByNumberAsync(int walletNumber);
    Task<Wallet> CreateNewWalletAsync(Guid? userId = null);
    Task<Wallet> AddCurrencyAccountAsync(
        Guid userId,
        Currency currency,
        bool isDefault = false);

    Task<Wallet> SetDefaultCurrencyAccountAsync(
        Guid userId,
        Currency currency);

    Task<Wallet> UpdateCurrencyAccountsAsync(Guid walletId,
        HashSet<CurrencyAccount> currencyAccounts,
        IClientSessionHandle? clientSessionHandle = null);
}
