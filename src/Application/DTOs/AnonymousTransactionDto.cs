using Defender.Common.DB.SharedStorage.Enums;

namespace Defender.WalletService.Application.DTOs;

public class AnonymousTransactionDto
{
    public string? TransactionId { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public TransactionPurpose TransactionPurpose { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime UtcTransactionDate { get; set; }

}
