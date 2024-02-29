using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Module.Commands;

public record StartTransferTransactionCommand : IRequest<Transaction>
{
    public int ToWalletNumber { get; set; }
    public int Amount { get; set; }
    public Currency Currency { get; set; }
};

public sealed class StartTransferTransactionCommandValidator :
    AbstractValidator<StartTransferTransactionCommand>
{
    public StartTransferTransactionCommandValidator()
    {
    }
}

public sealed class StartTransferTransactionCommandHandler :
    IRequestHandler<StartTransferTransactionCommand, Transaction>
{
    private readonly ICurrentAccountAccessor _currentAccountAccessor;
    private readonly ITransactionManagementService _transactionManagementService;
    private readonly IWalletManagementService _walletManagementService;

    public StartTransferTransactionCommandHandler(
        ICurrentAccountAccessor currentAccountAccessor,
        ITransactionManagementService transactionManagementService,
        IWalletManagementService walletManagementService
        )
    {
        _currentAccountAccessor = currentAccountAccessor;
        _transactionManagementService = transactionManagementService;
        _walletManagementService = walletManagementService;
    }

    public async Task<Transaction> Handle(
        StartTransferTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentAccountAccessor.GetAccountId();

        var currentUserWallet = await _walletManagementService
            .GetWalletByUserIdAsync(userId);

        var targetWallet = await _walletManagementService
            .GetWalletByNumberAsync(request.ToWalletNumber);

        if (targetWallet == null || currentUserWallet == null)
        {
            throw new ServiceException(ErrorCode.BR_WLT_WalletIsNotExist);
        }

        var transaction = await _transactionManagementService
                    .CreateTransferTransactionAsync(
                        currentUserWallet.WalletNumber,
                        targetWallet.WalletNumber,
                        request.Amount,
                        request.Currency);

        return transaction;
    }
}
