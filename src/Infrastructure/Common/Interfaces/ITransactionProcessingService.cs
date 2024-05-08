using Defender.WalletService.Infrastructure.Models;

namespace Defender.WalletService.Infrastructure.Common.Interfaces;

public interface ITransactionProcessingService
{
    Task<bool> ProcessTransaction(
        TransactionEvent transaction);
}
