using Defender.Common.Entities;
using Defender.WalletService.Domain.Consts;
using Defender.WalletService.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Defender.Common.DB.SharedStorage.Enums;

namespace Defender.WalletService.Domain.Entities.Transactions;

public class Transaction : IBaseModel
{
    public Guid Id { get; set; }

    public string? TransactionId { get; set; }
    public string? ParentTransactionId { get; set; }
    [BsonRepresentation(BsonType.String)]
    public TransactionStatus TransactionStatus { get; set; }
    [BsonRepresentation(BsonType.String)]
    public TransactionType TransactionType { get; set; }
    [BsonRepresentation(BsonType.String)]
    public TransactionPurpose TransactionPurpose { get; set; }
    public int FromWallet { get; set; }
    public int ToWallet { get; set; }
    public int Amount { get; set; }
    public DateTime UtcTransactionDate { get; set; }
    [BsonRepresentation(BsonType.String)]
    public Currency Currency { get; set; }
    public string? FailureCode { get; set; }
    public string? Comment { get; set; }


    #region Creators 

    public class CreateTransactionRequest
    {
        public int TargetWallet { get; set; }
        public Currency Currency { get; set; }
        public int Amount { get; set; }
        public string? Comment { get; set; }
        public TransactionPurpose TransactionPurpose { get; set; }
            = TransactionPurpose.NoPurpose;
    }

    public static Transaction CreateRecharge(CreateTransactionRequest request)
    {
        var transaction = CreateBaseTransaction(
            TransactionType.Recharge,
            request);

        transaction.ToWallet = request.TargetWallet;

        return transaction;
    }

    public static Transaction CreateTransfer(CreateTransactionRequest request, int fromWallet)
    {
        var transaction = CreateBaseTransaction(
            TransactionType.Transfer,
            request);

        transaction.ToWallet = request.TargetWallet;
        transaction.FromWallet = fromWallet;

        return transaction;
    }

    public static Transaction CreatePayment(
        CreateTransactionRequest request)
    {
        var transaction = CreateBaseTransaction(
            TransactionType.Payment,
            request);

        transaction.FromWallet = request.TargetWallet;

        return transaction;
    }

    public static Transaction CreateCancelation(
    Transaction parentTransaction)
    {
        var utcDateTime = DateTime.UtcNow;

        return new Transaction()
        {
            TransactionId = GenerateTransactionId(utcDateTime, TransactionType.Revert),
            ParentTransactionId = parentTransaction.TransactionId,
            TransactionType = TransactionType.Revert,
            TransactionPurpose = parentTransaction.TransactionPurpose,
            TransactionStatus = TransactionStatus.Queued,
            UtcTransactionDate = utcDateTime,
            Currency = parentTransaction.Currency,
            Amount = parentTransaction.Amount,
            ToWallet = parentTransaction.FromWallet,
            FromWallet = parentTransaction.ToWallet,
            Comment = parentTransaction.Comment,
        };
    }

    private static Transaction CreateBaseTransaction(
        TransactionType type,
        CreateTransactionRequest request)
    {
        var utcDateTime = DateTime.UtcNow;

        return new Transaction()
        {
            TransactionId = GenerateTransactionId(utcDateTime, type),
            TransactionType = type,
            TransactionPurpose = request.TransactionPurpose,
            TransactionStatus = TransactionStatus.Queued,
            UtcTransactionDate = utcDateTime,
            Currency = request.Currency,
            Amount = request.Amount,
            ToWallet = ConstantValues.NoWallet,
            FromWallet = ConstantValues.NoWallet,
            Comment = request.Comment,
        };
    }

    protected static string GenerateTransactionId(
        DateTime transactionDate,
        TransactionType type)
    {
        var prefixMap = new Dictionary<TransactionType, string>()
        {
            { TransactionType.Payment, "PAY" },
            { TransactionType.Recharge, "RCH" },
            { TransactionType.Transfer, "TRN" },
            { TransactionType.Revert, "CNL" },
        };

        return string.Format(
            "{0}-{1}-{2}",
            prefixMap[type],
            $"{transactionDate:yyyyMMddHHmm}",
            new Random().Next(1000, 9999));
    }


    #endregion

}


