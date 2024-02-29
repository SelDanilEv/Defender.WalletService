using Defender.WalletService.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Defender.WalletService.Domain.Entities.Wallets;

public class CurrencyAccount
{
    public CurrencyAccount()
    {
    }
    public CurrencyAccount(Currency currency, bool isDefault = false)
    {
        Currency = currency;
        Balance = 0;
        IsDefault = isDefault;
    }

    [BsonRepresentation(BsonType.String)]
    public Currency Currency { get; set; }
    public int Balance { get; set; }
    public bool IsDefault { get; set; }
}
