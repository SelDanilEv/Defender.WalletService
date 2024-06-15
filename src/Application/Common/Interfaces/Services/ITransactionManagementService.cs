using Defender.Common.DB.Pagination;
using Defender.Common.DB.SharedStorage.Enums;
using Defender.Common.Errors;
using Defender.WalletService.Domain.Entities.Transactions;

namespace Defender.WalletService.Application.Common.Interfaces.Services;

public interface ITransactionManagementService
{
    Task<PagedResult<Transaction>> GetTransactionsByWalletNumberAsync(
        PaginationRequest paginationRequest,
        int walletNumber);

    Task<Transaction> GetTransactionByTransactionIdAsync(
        string transactionId);

    Task<Transaction> UpdateTransactionStatusAsync(
        Transaction transaction,
        TransactionStatus newStatus,
        string? comment = null);

    Task<Transaction> UpdateTransactionStatusAsync(
        Transaction transaction,
        TransactionStatus newStatus,
        ErrorCode error);

    Task<Transaction> CreateRechargeTransactionAsync(
        Transaction.CreateTransactionRequest request);

    Task<Transaction> CreateTransferTransactionAsync(
        int fromWallet,
        Transaction.CreateTransactionRequest request);

    Task<Transaction> CreatePaymentTransactionAsync(
        Transaction.CreateTransactionRequest request);

    Task<Transaction> CancelTransactionAsync(
        string transactionId);

}
