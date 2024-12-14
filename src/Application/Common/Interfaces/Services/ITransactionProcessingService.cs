namespace Defender.WalletService.Application.Common.Interfaces.Services;

public interface ITransactionProcessingService
{
    Task<bool> ProcessTransaction(
        string transactionId);
}
