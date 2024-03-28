using Defender.Common.DB.Pagination;
using Defender.Common.Errors;
using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Enums;

namespace Defender.WalletService.Application.Common.Interfaces;

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
        int wallet, 
        int amount, 
        Currency currency);

    Task<Transaction> CreateTransferTransactionAsync(
        int fromWallet, 
        int toWallet,
        int amount,
        Currency currency);

    Task<Transaction> CreatePaymentTransactionAsync(
        int wallet,
        int amount, 
        Currency currency);

}
