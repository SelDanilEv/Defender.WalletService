using Defender.WalletService.Domain.Enums;

namespace Defender.WalletService.Domain.Entities.Wallets;

public class BaseWallet
{
    public int WalletNumber { get; set; }

    public HashSet<CurrencyAccount> CurrencyAccounts { get; set; }
        = new HashSet<CurrencyAccount>();

    public CurrencyAccount GetDefaultCurrencyAccount() =>
        CurrencyAccounts.FirstOrDefault(x => x.IsDefault)!;

    public bool IsCurrencyAccountExist(Currency currency) =>
        CurrencyAccounts.Any(x => x.Currency == currency);

    public CurrencyAccount GetCurrencyAccount(Currency currency) =>
        CurrencyAccounts?.FirstOrDefault(x => x.Currency == currency)!;
}


