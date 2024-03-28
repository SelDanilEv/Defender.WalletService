using Defender.WalletService.Domain.Enums;

namespace Defender.WalletService.Application.Modules.Transactions.Commands;

public record BaseTransactionCommand
{
    public int Amount { get; set; }
    public Currency Currency { get; set; }
    public string? Comment { get; set; }
}
