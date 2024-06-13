using Defender.WalletService.Application.Events;

namespace Defender.WalletService.Application.Common.Interfaces.Services;

public interface ITransactionProcessingService
{
    Task<bool> ProcessTransaction(
        NewTransactionCreatedEvent transaction);
}
