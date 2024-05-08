using Defender.Common.Errors;
using Defender.Common.Helpers;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Enums;
using Defender.WalletService.Infrastructure.Common.Interfaces;
using Defender.WalletService.Infrastructure.Mappings;
using Defender.WalletService.Infrastructure.Models;
using MongoDB.Driver;

namespace Defender.WalletService.Infrastructure.Services;

public class TransactionProcessingService : ITransactionProcessingService
{
    private readonly ITransactionManagementService _transactionManagementService;
    private readonly IWalletRepository _walletRepository;

    private readonly TransactionTypeActionMapper _transactionTypeMap =
        [];

    public TransactionProcessingService(
        ITransactionManagementService transactionManagementService,
        IWalletRepository walletRepository)
    {
        _transactionManagementService = transactionManagementService;
        _walletRepository = walletRepository;

        _transactionTypeMap = new TransactionTypeActionMapper
            {
                {TransactionType.Recharge, ProcessRecharge },
                {TransactionType.Transfer, ProcessTransfer },
                {TransactionType.Payment, ProcessPayment }
            };
    }

    public async Task<bool> ProcessTransaction(TransactionEvent transactionEvent)
    {
        if (transactionEvent == null || string.IsNullOrWhiteSpace(transactionEvent.TransactionId))
            return true;

        var transaction = await _transactionManagementService
            .GetTransactionByTransactionIdAsync(transactionEvent.TransactionId);

        if(transaction == null || transaction.TransactionStatus != TransactionStatus.Queued)
            return true;

        try
        {
            var mongoSession = await _walletRepository.StartSessionAsync();

            _transactionTypeMap.TryGetValue(
                    transaction.TransactionType, out var processAction);

            if (processAction == null)
            {
                transaction = await _transactionManagementService
                    .UpdateTransactionStatusAsync(transaction, TransactionStatus.Failed);

                return true;
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
            await _transactionManagementService
                .UpdateTransactionStatusAsync(transaction, TransactionStatus.Failed);

            return true;
        }


        return transaction.TransactionStatus != TransactionStatus.Queued;
    }

    private async Task ProcessRecharge(
        Transaction transaction,
        IClientSessionHandle sessionHandle)
    {
        await ProcessDebitAsync(transaction, sessionHandle);

        await _transactionManagementService
            .UpdateTransactionStatusAsync(
                transaction,
                TransactionStatus.Proceed);
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

        var isStepSuccess =
            await ProcessCreditAsync(transaction, sessionHandle);
        if (isStepSuccess)
            await ProcessDebitAsync(transaction, sessionHandle);

        if (isStepSuccess)
            await _transactionManagementService
            .UpdateTransactionStatusAsync(
                transaction,
                TransactionStatus.Proceed);
    }

    private async Task ProcessPayment(
        Transaction transaction,
        IClientSessionHandle sessionHandle)
    {
        await ProcessCreditAsync(transaction, sessionHandle);

        await _transactionManagementService
            .UpdateTransactionStatusAsync(
                transaction,
                TransactionStatus.Proceed);
    }

    private async Task<bool> ProcessDebitAsync(
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
            return false;
        }

        if (!toWallet.IsCurrencyAccountExist(transaction.Currency))
        {
            await HandleError(
                transaction,
                ErrorCode.BR_WLT_RecipientCurrencyAccountIsNotExist);
            return false;
        };

        var currencyAccount = toWallet.GetCurrencyAccount(transaction.Currency);

        currencyAccount.Balance += transaction.Amount;

        await _walletRepository.UpdateCurrencyAccountsAsync(
            toWallet.Id,
            toWallet.CurrencyAccounts,
            sessionHandle);

        return true;
    }

    private async Task<bool> ProcessCreditAsync(
        Transaction transaction,
        IClientSessionHandle sessionHandle)
    {
        var fromWallet = await _walletRepository
        .GetWalletByNumberAsync(transaction.FromWallet);

        if (fromWallet == null)
        {
            await HandleError(
                transaction,
                ErrorCode.BR_WLT_WalletIsNotExist);
            return false;
        };

        if (!fromWallet.IsCurrencyAccountExist(transaction.Currency))
        {
            await HandleError(
                transaction,
                ErrorCode.BR_WLT_SenderCurrencyAccountIsNotExist);
            return false;
        };

        var currencyAccount = fromWallet
            .GetCurrencyAccount(transaction.Currency);

        currencyAccount.Balance -= transaction.Amount;

        if (currencyAccount.Balance < 0)
        {
            await HandleError(
                transaction,
                ErrorCode.BR_WLT_NotEnoughFunds);
            return false;
        }

        await _walletRepository.UpdateCurrencyAccountsAsync(
            fromWallet.Id,
            fromWallet.CurrencyAccounts,
            sessionHandle);

        return true;
    }

    private static Func<Task> MapToFunc<T>(
        Func<T, IClientSessionHandle, Task> func,
        T parameter,
        IClientSessionHandle sessionHandle)
    {
        return async () => await func(parameter, sessionHandle);
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
