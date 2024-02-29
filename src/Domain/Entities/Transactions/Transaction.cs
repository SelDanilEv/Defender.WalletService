using Defender.Common.Entities;
using Defender.WalletService.Domain.Consts;
using Defender.WalletService.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.WalletService.Domain.Entities.Transactions;

public class Transaction : BaseTopicMessage, IBaseModel
{
    public Guid Id { get; set; }

    public string? TransactionId { get; set; }
    [BsonRepresentation(BsonType.String)]
    public TransactionStatus TransactionStatus { get; set; }
    [BsonRepresentation(BsonType.String)]
    public TransactionType TransactionType { get; set; }
    public int FromWallet { get; set; }
    public int ToWallet { get; set; }
    public int Amount { get; set; }
    public DateTime UtcTransactionDate { get; set; }
    [BsonRepresentation(BsonType.String)]
    public Currency Currency { get; set; }


    #region Creators 

    public static Transaction CreateRecharge(
        Currency currency,
        int amount,
        int toWallet)
    {
        var transaction = CreateBaseTransaction(
            TransactionType.Recharge,
            currency,
            amount);

        transaction.ToWallet = toWallet;

        return transaction;
    }

    public static Transaction CreateTransfer(
    Currency currency,
    int amount,
    int toWallet,
    int fromWallet)
    {
        var transaction = CreateBaseTransaction(
            TransactionType.Transfer,
            currency,
            amount);

        transaction.ToWallet = toWallet;
        transaction.FromWallet = fromWallet;

        return transaction;
    }

    public static Transaction CreatePayment(
    Currency currency,
    int amount,
    int fromWallet)
    {
        var transaction = CreateBaseTransaction(
            TransactionType.Payment,
            currency,
            amount);

        transaction.FromWallet = fromWallet;

        return transaction;
    }

    private static Transaction CreateBaseTransaction(
            TransactionType type,
            Currency currency,
            int amount)
    {
        var utcDateTime = DateTime.UtcNow;

        return new Transaction()
        {
            TransactionId = GenerateTransactionId(utcDateTime,type),
            TransactionType = type,
            TransactionStatus = TransactionStatus.Queued,
            UtcTransactionDate = utcDateTime,
            Currency = currency,
            Amount = amount,
            ToWallet = ConstantValues.NoWallet,
            FromWallet = ConstantValues.NoWallet,
        };
    }

    private static string GenerateTransactionId(
        DateTime transactionDate, 
        TransactionType type)
    {
        var prefixMap = new Dictionary<TransactionType, string>()
        {
            { TransactionType.Payment, "PAY" },
            { TransactionType.Recharge, "RCH" },
            { TransactionType.Transfer, "TRN" },
        };

        return string.Format(
            "{0}-{1}-{2}", 
            prefixMap[type],
            $"{transactionDate:yyyyMMddHHmmss}", 
            new Random().Next(10000, 99999));
    }


    #endregion


    #region Methods


    public Transaction MapToUserTransaction(int userWalletNumber)
    {
        if(userWalletNumber == this.FromWallet)
        {
            this.Amount *= -1;
        }

        return this;
    }


    #endregion
}


