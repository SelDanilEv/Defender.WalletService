using Defender.Common.DB.Model;
using Defender.Common.DB.Pagination;
using Defender.Common.DB.SharedStorage.Entities;
using Defender.Common.DB.SharedStorage.Enums;
using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.Common.Kafka;
using Defender.Common.Kafka.Default;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Common.Kafka;
using Defender.WalletService.Domain.Entities.Transactions;

namespace Defender.WalletService.Application.Services;

public class TransactionManagementService(
    ITransactionRepository transactionRepository,
    IDefaultKafkaProducer<string> newTransactionsProducer,
    IDefaultKafkaProducer<TransactionStatusUpdatedEvent> updatedStatusesProducer
)
    : ITransactionManagementService
{
    public async Task<PagedResult<Transaction>> GetTransactionsByWalletNumberAsync(
        PaginationRequest paginationRequest,
        int walletNumber)
    {
        var transactions = await transactionRepository
            .GetTransactionsAsync(paginationRequest, walletNumber);

        return transactions;
    }

    public async Task<Transaction> GetTransactionByTransactionIdAsync(
        string transactionId)
    {
        return await transactionRepository
            .GetTransactionByIdAsync(transactionId);
    }

    public async Task<Transaction> UpdateTransactionStatusAsync(
        Transaction transaction,
        TransactionStatus newStatus,
        string? failureCode = null)
    {
        if (newStatus < transaction.TransactionStatus)
        {
            throw new ServiceException(ErrorCode.BR_WLT_InvalidTransactionStatus);
        }

        if (newStatus == transaction.TransactionStatus && string.IsNullOrWhiteSpace(failureCode))
        {
            return transaction;
        }

        var request = UpdateModelRequest<Transaction>
            .Init(transaction)
            .SetIfNotNull(x => x.TransactionStatus, newStatus)
            .SetIfNotNull(x => x.FailureCode, failureCode);

        var result = await transactionRepository
            .UpdateTransactionAsync(request);

        var statusUpdatedEvent = new TransactionStatusUpdatedEvent
        {
            TransactionId = result.TransactionId,
            TransactionStatus = result.TransactionStatus,
            TransactionType = result.TransactionType,
            TransactionPurpose = result.TransactionPurpose,
        };

        await updatedStatusesProducer.ProduceAsync(
            Topic.TransactionStatusUpdates.GetName(),
            statusUpdatedEvent,
            CancellationToken.None);

        return result;
    }

    public async Task<Transaction> UpdateTransactionStatusAsync(
        Transaction transaction,
        TransactionStatus newStatus,
        ErrorCode error)
    {
        return await UpdateTransactionStatusAsync(
            transaction,
            newStatus,
            error.ToString());
    }

    public async Task<Transaction> CreatePaymentTransactionAsync(
        Transaction.CreateTransactionRequest request)
    {
        var transaction = Transaction.CreatePayment(request);

        return await CreateTransactionAsync(transaction);
    }

    public async Task<Transaction> CreateRechargeTransactionAsync(
        Transaction.CreateTransactionRequest request)
    {
        var transaction = Transaction.CreateRecharge(request);

        return await CreateTransactionAsync(transaction);
    }

    public async Task<Transaction> CreateTransferTransactionAsync(
        int fromWallet,
        Transaction.CreateTransactionRequest request)
    {
        var transaction = Transaction.CreateTransfer(request, fromWallet);

        return await CreateTransactionAsync(transaction);
    }

    public async Task<Transaction> CancelTransactionAsync(
        string transactionId)
    {
        var originalTransaction = await GetTransactionByTransactionIdAsync(transactionId);

        switch (originalTransaction.TransactionStatus)
        {
            case TransactionStatus.Queued:
                await UpdateTransactionStatusAsync(originalTransaction, TransactionStatus.Canceled);

                return originalTransaction;
            case TransactionStatus.Proceed:
                {
                    var transaction = Transaction.CreateCancelation(originalTransaction);

                    _ = UpdateTransactionStatusAsync(
                        originalTransaction,
                        TransactionStatus.QueuedForRevert);

                    return await CreateTransactionAsync(transaction);
                }
            default:
                throw new ServiceException(ErrorCode.BR_WLT_TransactionCanNotBeCanceled);
        }
    }

    private async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        transaction = await transactionRepository
            .CreateNewTransactionAsync(transaction);

        await newTransactionsProducer.ProduceAsync(
            KafkaTopic.TransactionsToProcess.GetName(),
            transaction.TransactionId,
            CancellationToken.None);

        return transaction;
    }
}