using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.Common.Extension;
using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Entities.Wallets;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Transactions.Commands;

public record StartPaymentTransactionCommand : BaseTransactionCommand, IRequest<Transaction>
{
};

public sealed class StartPaymentTransactionCommandValidator :
    AbstractValidator<StartPaymentTransactionCommand>
{
    public StartPaymentTransactionCommandValidator()
    {
        When(x => !x.TargetUserId.HasValue, () =>
            RuleFor(x => x.TargetWalletNumber)
                .NotEmpty()
                .WithMessage(ErrorCode.VL_WLT_EmptyWalletNumber)
                .InclusiveBetween(10000000, 99999999)
                .WithMessage(ErrorCode.VL_WLT_InvalidWalletNumber));

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage(ErrorCode.VL_WLT_TransferAmountMustBePositive);
    }
}

public sealed class StartPaymentTransactionCommandHandler(
        IAuthorizationCheckingService authorizationCheckingService,
        ICurrentAccountAccessor currentAccountAccessor,
        ITransactionManagementService transactionManagementService,
        IWalletManagementService walletManagementService
        ) :
    IRequestHandler<StartPaymentTransactionCommand, Transaction>
{

    public async Task<Transaction> Handle(
        StartPaymentTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentAccountAccessor.GetAccountId();

        var currentUserWallet = await walletManagementService
            .GetWalletByUserIdAsync(userId);

        Wallet? targetWallet = null;

        if (request.TargetUserId.HasValue)
        {
            targetWallet = await walletManagementService
                .GetWalletByUserIdAsync(request.TargetUserId.Value);

            if (targetWallet == null)
                throw new ServiceException(ErrorCode.BR_WLT_WalletIsNotExist);

            request.TargetWalletNumber = targetWallet.WalletNumber;
        }

        if (currentUserWallet.WalletNumber == request.TargetWalletNumber)
        {
            return await transactionManagementService
                .CreatePaymentTransactionAsync(
                    request.CreateTransactionRequest);
        }

        targetWallet ??= await walletManagementService
            .GetWalletByNumberAsync(request.TargetWalletNumber);

        if (targetWallet == null)
            throw new ServiceException(ErrorCode.BR_WLT_WalletIsNotExist);

        return await authorizationCheckingService
                .ExecuteWithAuthCheckAsync(
                    targetWallet.Id,
                    async () => await transactionManagementService
                        .CreatePaymentTransactionAsync(
                            request.CreateTransactionRequest));
    }
}
