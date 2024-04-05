using Defender.WalletService.Domain.Enums;

namespace Defender.WalletService.Application.DTOs;

public class TransactionDto
{
    public string? TransactionId { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public TransactionType TransactionType { get; set; }
    public Currency Currency { get; set; }

    public int Amount { get; set; }
    public DateTime UtcTransactionDate { get; set; }

    public int FromWallet { get; set; }
    public int ToWallet { get; set; }

    public string? FailureCode { get; set; }
    public string? Comment { get; set; }
}
