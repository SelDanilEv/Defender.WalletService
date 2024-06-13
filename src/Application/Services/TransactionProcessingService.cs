using Defender.Common.Errors;
using Defender.Common.Helpers;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Consts;
using MongoDB.Driver;
using Defender.Common.DB.SharedStorage.Enums;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Application.Events;
using Defender.WalletService.Application.Mappings;

namespace Defender.WalletService.Application.Services;

public class TransactionProcessingService : ITransactionProcessingService
{
    private const int WriteConflictErrorCode = 112;
    private readonly ITransactionManagementService _transactionManagementService;
    private readonly IWalletRepository _walletRepository;

    private readonly TransactionTypeActionMapper _transactionTypeMap = [];

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
                {TransactionType.Payment, ProcessPayment },
                {TransactionType.Revert, ProcessRevert }
            };
    }

    public async Task<bool> ProcessTransaction(NewTransactionCreatedEvent transactionEvent)
    {
        if (transactionEvent == null
            || string.IsNullOrWhiteSpace(transactionEvent.TransactionId))
            return true;

        var transaction = await _transactionManagementService
            .GetTransactionByTransactionIdAsync(transactionEvent.TransactionId);

        if (transaction == null || transaction.TransactionStatus != TransactionStatus.Queued)
            return true;

        try
        {
            var mongoSession = await _walletRepository.StartSessionAsync();

            _transactionTypeMap.TryGetValue(
                    transaction.TransactionType, out var processAction);

            if (processAction == null)
            {
                transaction = await HandleError(transaction, ErrorCode.UnhandledError);

                return true;
            }

            var (isSucceeded, er) = await MongoTransactionHelper
                .ExecuteUnderTransactionWithExceptionAsync(
                    mongoSession,
                    MapToFunc(processAction, transaction, mongoSession));

            if (isSucceeded) return true;

            if (er != null && er.Code == WriteConflictErrorCode) return false;

            transaction = await HandleError(transaction, ErrorCode.UnhandledError);
        }
        catch
        {
            await HandleError(transaction, ErrorCode.UnhandledError);

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

    private async Task ProcessRevert(
        Transaction transaction,
        IClientSessionHandle sessionHandle)
    {
        var isStepSuccess = transaction.FromWallet != ConstantValues.NoWallet ?
            await ProcessCreditAsync(transaction, sessionHandle) : true;
        if (isStepSuccess && transaction.ToWallet != ConstantValues.NoWallet)
            await ProcessDebitAsync(transaction, sessionHandle);

        if (isStepSuccess)
        {
            await _transactionManagementService
                .UpdateTransactionStatusAsync(
                    transaction,
                    TransactionStatus.Proceed);

            var originalTransaction = await _transactionManagementService
                .GetTransactionByTransactionIdAsync(transaction.ParentTransactionId);

            await _transactionManagementService
                .UpdateTransactionStatusAsync(
                    originalTransaction,
                    TransactionStatus.Reverted);
        }
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
