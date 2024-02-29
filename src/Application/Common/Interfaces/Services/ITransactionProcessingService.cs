using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Enums;

namespace Defender.WalletService.Application.Common.Interfaces;

public interface ITransactionProcessingService
{
    Task<TransactionStatus> ProcessTransaction(
        Transaction transaction);
}
