using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.Common.Helpers;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Enums;
using MongoDB.Driver;

namespace Defender.WalletService.Infrastructure.Services;

public class TransactionProcessingService : ITransactionProcessingService
{
    private readonly ITransactionManagementService _transactionManagementService;
    private readonly IWalletRepository _walletRepository;

    private readonly Dictionary<TransactionType, Func<Transaction, IClientSessionHandle, Task>> transactionTypeMap;

    public TransactionProcessingService(
        ITransactionManagementService transactionManagementService,
        IWalletRepository walletRepository)
    {
        _transactionManagementService = transactionManagementService;
        _walletRepository = walletRepository;

        transactionTypeMap = new Dictionary<TransactionType, Func<Transaction, IClientSessionHandle, Task>>
        {
            { TransactionType.Recharge, ProcessRecharge },
            { TransactionType.Transfer, ProcessTransfer },
            { TransactionType.Payment, ProcessPayment }
        };
    }

    public async Task<TransactionStatus> ProcessTransaction(
        Transaction transaction)
    {
        var mongoSession = await _walletRepository.StartSessionAsync();

        transactionTypeMap.TryGetValue(transaction.TransactionType, out var processAction);

        if (processAction == null)
        {
            await _transactionManagementService
                .UpdateTransactionStatusAsync(transaction, TransactionStatus.Failed);

            return transaction.TransactionStatus;
        }

        var isSucceeded = await MongoTransactionHelper
            .ExecuteUnderTransactionAsync(
                mongoSession,
                MapToFunc(processAction, transaction, mongoSession));

        if (!isSucceeded)
        {
            await _transactionManagementService
                .UpdateTransactionStatusAsync(transaction, TransactionStatus.Failed);

            return transaction.TransactionStatus;
        }

        return transaction.TransactionStatus;
    }

    private async Task ProcessRecharge(
        Transaction transaction,
        IClientSessionHandle sessionHandle)
    {
        await ProccessDebitAsync(transaction, sessionHandle);

        await _transactionManagementService.UpdateTransactionStatusAsync(transaction, TransactionStatus.Procced);
    }

    private async Task ProcessTransfer(
        Transaction transaction,
        IClientSessionHandle sessionHandle)
    {
        await ProccessCreditAsync(transaction, sessionHandle);
        await ProccessDebitAsync(transaction, sessionHandle);

        await _transactionManagementService.UpdateTransactionStatusAsync(transaction, TransactionStatus.Procced);
    }

    private async Task ProcessPayment(
        Transaction transaction,
        IClientSessionHandle sessionHandle)
    {
        await ProccessCreditAsync(transaction, sessionHandle);

        await _transactionManagementService.UpdateTransactionStatusAsync(transaction, TransactionStatus.Procced);
    }

    private async Task ProccessDebitAsync(
        Transaction transaction,
        IClientSessionHandle sessionHandle)
    {
        var toWallet = await _walletRepository.GetWalletByNumberAsync(transaction.ToWallet);

        if (!toWallet.IsCurrencyAccountExist(transaction.Currency))
        {
            throw new ServiceException(ErrorCode.BR_WLT_RecipientCurrencyAccountIsNotExist);
        };

        var currencyAccount = toWallet.GetCurrencyAccount(transaction.Currency);

        currencyAccount.Balance += transaction.Amount;

        await _walletRepository.UpdateCurrencyAccountsAsync(
            toWallet.Id,
            toWallet.CurrencyAccounts,
            sessionHandle);
    }

    private async Task ProccessCreditAsync(
        Transaction transaction,
        IClientSessionHandle sessionHandle)
    {
        var fromWallet = await _walletRepository.GetWalletByNumberAsync(transaction.FromWallet);

        if (!fromWallet.IsCurrencyAccountExist(transaction.Currency))
        {
            throw new ServiceException(ErrorCode.BR_WLT_SenderCurrencyAccountIsNotExist);
        };

        var currencyAccount = fromWallet.GetCurrencyAccount(transaction.Currency);

        currencyAccount.Balance -= transaction.Amount;

        if (currencyAccount.Balance < 0)
        {
            throw new ServiceException(ErrorCode.BR_WLT_NotEnoughFunds);
        }

        await _walletRepository.UpdateCurrencyAccountsAsync(
            fromWallet.Id,
            fromWallet.CurrencyAccounts,
            sessionHandle);
    }

    private static Func<Task> MapToFunc<T>(
        Func<T, IClientSessionHandle, Task> func,
        T parameter,
        IClientSessionHandle sessionHandle)
    {
        return async () =>
        {
            await func(parameter, sessionHandle);
        };
    }
}
