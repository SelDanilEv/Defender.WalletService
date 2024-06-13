using System.Text.Json.Serialization;
using Defender.Common.DB.SharedStorage.Enums;
using Defender.WalletService.Domain.Consts;
using Defender.WalletService.Domain.Enums;
using static Defender.WalletService.Domain.Entities.Transactions.Transaction;

namespace Defender.WalletService.Application.Modules.Transactions.Commands;

public record BaseTransactionCommand
{
    public Guid? TargetUserId { get; set; }
    public int TargetWalletNumber { get; set; } = ConstantValues.NoWallet;
    public int Amount { get; set; }
    public Currency Currency { get; set; }
    public string? Comment { get; set; }
    public TransactionPurpose TransactionPurpose { get; set; }
        = TransactionPurpose.NoPurpose;

    [JsonIgnore]
    public CreateTransactionRequest CreateTransactionRequest =>
        new()
        {
            TargetWallet = TargetWalletNumber,
            Amount = Amount,
            Currency = Currency,
            Comment = Comment,
            TransactionPurpose = TransactionPurpose
        };
}
