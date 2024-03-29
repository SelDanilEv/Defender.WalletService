﻿using Defender.Common.DB.Model;
using Defender.Common.DB.Pagination;
using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.Mongo.MessageBroker.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Enums;
using Defender.WalletService.Infrastructure.Consts;

namespace Defender.WalletService.Infrastructure.Services;

public class TransactionManagementService : ITransactionManagementService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IProducer _messageProducer;

    public TransactionManagementService(
        ITransactionRepository transactionRepository,
        IProducer messageProducer)
    {
        _transactionRepository = transactionRepository;

        _messageProducer = messageProducer;

        _messageProducer.SetTopic(MessageBroker.Topics.TransactionTopic);
        _messageProducer.SetMessageType(MessageBroker.MessageTypes.Transaction);
    }

    public async Task<PagedResult<Transaction>> GetTransactionsByWalletNumberAsync(
        PaginationRequest paginationRequest,
        int walletNumber)
    {
        var transactions = await _transactionRepository
            .GetTransactionsAsync(paginationRequest, walletNumber);

        transactions.Items = transactions
            .Items
            .Select(t => t.MapToUserTransaction(walletNumber))
            .ToList();

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
        TransactionStatus newStatus)
    {
        if (newStatus <= transaction.TransactionStatus)
        {
            throw new ServiceException(ErrorCode.BR_WLT_InvalidTransactionStatus);
        }

        var request = UpdateModelRequest<Transaction>
            .Init(transaction)
            .SetIfNotNull(x => x.TransactionStatus, newStatus);

        return await _transactionRepository
            .UpdateTransactionAsync(request);
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

        await _messageProducer.PublishAsync(transaction);

        return transaction;
    }

}
