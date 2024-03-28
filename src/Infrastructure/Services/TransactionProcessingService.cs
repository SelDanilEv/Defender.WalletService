using Defender.Common.Errors;
using Defender.Common.Helpers;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Enums;
using Defender.WalletService.Infrastructure.Mappings;
using MongoDB.Driver;

namespace Defender.WalletService.Infrastructure.Services;

public class TransactionProcessingService : ITransactionProcessingService
{
    private readonly ITransactionManagementService _transactionManagementService;
    private readonly IWalletRepository _walletRepository;

    private readonly TransactionTypeActionMapper transactionTypeMap = 
        new TransactionTypeActionMapper();

    public TransactionProcessingService(
        ITransactionManagementService transactionManagementService,
        IWalletRepository walletRepository)
    {
        _transactionManagementService = transactionManagementService;
        _walletRepository = walletRepository;

        transactionTypeMap.Add(TransactionType.Recharge, ProcessRecharge);
        transactionTypeMap.Add(TransactionType.Transfer, ProcessTransfer);
        transactionTypeMap.Add(TransactionType.Payment, ProcessPayment);
    }

    public async Task<TransactionStatus> ProcessTransaction(
        Transaction transaction)
    {
        try
        {
            var mongoSession = await _walletRepository.StartSessionAsync();

            transactionTypeMap.TryGetValue(
                    transaction.TransactionType, out var processAction);

            if (processAction == null)
            {
                transaction = await _transactionManagementService
                    .UpdateTransactionStatusAsync(transaction, TransactionStatus.Failed);

                return transaction.TransactionStatus;
            }

            var isSucceeded = await MongoTransactionHelper
                .ExecuteUnderTransactionAsync(
                    mongoSession,
                    MapToFunc(processAction, transaction, mongoSession));

            if (!isSucceeded)
            {
                transaction = await _transactionManagementService
                    .UpdateTransactionStatusAsync(transaction, TransactionStatus.Failed);
            }
        }
        catch
        {
            transaction = await _transactionManagementService
                .UpdateTransactionStatusAsync(transaction, TransactionStatus.Failed);
        }


        return transaction.TransactionStatus;
    }

    private async Task ProcessRecharge(
        Transaction transaction,
        IClientSessionHandle sessionHandle)
    {
        await ProccessDebitAsync(transaction, sessionHandle);

        await _transactionManagementService
            .UpdateTransactionStatusAsync(
                transaction,
                TransactionStatus.Procced);
    }

    private async Task ProcessTransfer(
        Transaction transaction,
        IClientSessionHandle sessionHandle)
    {
        if (transaction.FromWallet == transaction.ToWallet)
        {
            await HandleError(
                transaction,
                ErrorCode.BR_WLT_SenderAndRecipientAreTheSame);
            return;
        }

        await ProccessCreditAsync(transaction, sessionHandle);
        await ProccessDebitAsync(transaction, sessionHandle);

        await _transactionManagementService
            .UpdateTransactionStatusAsync(
                transaction,
                TransactionStatus.Procced);
    }

    private async Task ProcessPayment(
        Transaction transaction,
        IClientSessionHandle sessionHandle)
    {
        await ProccessCreditAsync(transaction, sessionHandle);

        await _transactionManagementService
            .UpdateTransactionStatusAsync(
                transaction,
                TransactionStatus.Procced);
    }

    private async Task ProccessDebitAsync(
        Transaction transaction,
        IClientSessionHandle sessionHandle)
    {
        var toWallet = await _walletRepository
            .GetWalletByNumberAsync(transaction.ToWallet);

        if (toWallet == null)
        {
            await HandleError(
                transaction,
                ErrorCode.BR_WLT_WalletIsNotExist);
            return;
        }

        if (!toWallet.IsCurrencyAccountExist(transaction.Currency))
        {
            await HandleError(
                transaction,
                ErrorCode.BR_WLT_RecipientCurrencyAccountIsNotExist);
            return;
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
        var fromWallet = await _walletRepository
            .GetWalletByNumberAsync(transaction.FromWallet);

        if (!fromWallet.IsCurrencyAccountExist(transaction.Currency))
        {
            await HandleError(
                transaction,
                ErrorCode.BR_WLT_SenderCurrencyAccountIsNotExist);
            return;
        };

        var currencyAccount = fromWallet
            .GetCurrencyAccount(transaction.Currency);

        currencyAccount.Balance -= transaction.Amount;

        if (currencyAccount.Balance < 0)
        {
            await HandleError(
                transaction,
                ErrorCode.BR_WLT_NotEnoughFunds);
            return;
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

    private async Task<Transaction> HandleError(
        Transaction transaction,
        ErrorCode error)
    {
        return await _transactionManagementService
            .UpdateTransactionStatusAsync(
            transaction,
            TransactionStatus.Failed,
            error);
    }
}
