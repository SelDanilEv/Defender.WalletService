using Defender.Common.DB.Model;
using Defender.Common.DB.Pagination;
using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Enums;
using Defender.WalletService.Infrastructure.Consts;
using Defender.WalletService.Infrastructure.Models;

namespace Defender.WalletService.Infrastructure.Services;

public class TransactionManagementService : ITransactionManagementService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IQueueProducer _messageProducer;

    public TransactionManagementService(
        ITransactionRepository transactionRepository,
        IQueueProducer messageProducer)
    {
        _transactionRepository = transactionRepository;

        _messageProducer = messageProducer;

        _messageProducer.SetQueue(MessageBroker.Queues.TransactionEventQueue);
        _messageProducer.SetMessageType(MessageBroker.MessageTypes.Transaction);
    }

    public async Task<PagedResult<Transaction>> GetTransactionsByWalletNumberAsync(
        PaginationRequest paginationRequest,
        int walletNumber)
    {
        var transactions = await _transactionRepository
            .GetTransactionsAsync(paginationRequest, walletNumber);

        return transactions;
    }

    public async Task<Transaction> GetTransactionByTransactionIdAsync(
        string transactionId)
    {
        return await _transactionRepository
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
        if (newStatus == transaction.TransactionStatus && String.IsNullOrWhiteSpace(failureCode))
        {
            return transaction;
        }

        var request = UpdateModelRequest<Transaction>
            .Init(transaction)
            .SetIfNotNull(x => x.TransactionStatus, newStatus)
            .SetIfNotNull(x => x.FailureCode, failureCode);

        return await _transactionRepository
            .UpdateTransactionAsync(request);
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
        int wallet,
        int amount,
        Currency currency)
    {
        var transaction = Transaction.CreatePayment(currency, amount, wallet);

        return await CreateTransactionBaseAsync(transaction);
    }

    public async Task<Transaction> CreateRechargeTransactionAsync(
        int wallet,
        int amount,
        Currency currency)
    {
        var transaction = Transaction.CreateRecharge(currency, amount, wallet);

        return await CreateTransactionBaseAsync(transaction);
    }

    public async Task<Transaction> CreateTransferTransactionAsync(
        int fromWallet,
        int toWallet,
        int amount,
        Currency currency)
    {
        var transaction = Transaction.CreateTransfer(currency, amount, toWallet, fromWallet);

        return await CreateTransactionBaseAsync(transaction);
    }

    private async Task<Transaction> CreateTransactionBaseAsync(Transaction transaction)
    {
        transaction = await _transactionRepository
            .CreateNewTransactionAsync(transaction);

        var transactionEvent = new TransactionEvent
        {
            TransactionId = transaction.TransactionId,
        };

        await _messageProducer.PublishQueueAsync(transactionEvent);

        return transaction;
    }

}
