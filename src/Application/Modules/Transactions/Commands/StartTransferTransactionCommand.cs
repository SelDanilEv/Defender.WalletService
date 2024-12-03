using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.Common.Extension;
using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Domain.Entities.Transactions;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Transactions.Commands;

public record StartTransferTransactionCommand : BaseTransactionCommand, IRequest<Transaction>
{
};

public sealed class StartTransferTransactionCommandValidator :
    AbstractValidator<StartTransferTransactionCommand>
{
    public StartTransferTransactionCommandValidator()
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

public sealed class StartTransferTransactionCommandHandler(
        ICurrentAccountAccessor currentAccountAccessor,
        ITransactionManagementService transactionManagementService,
        IWalletManagementService walletManagementService
        ) :
    IRequestHandler<StartTransferTransactionCommand, Transaction>
{
    public async Task<Transaction> Handle(
        StartTransferTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentAccountAccessor.GetAccountId();

        var currentUserWallet = await walletManagementService
            .GetWalletByUserIdAsync(userId)
        ?? throw new ServiceException(ErrorCode.BR_WLT_WalletIsNotExist);

        var targetWallet = request.TargetUserId.HasValue
            ? await walletManagementService
                .GetWalletByUserIdAsync(request.TargetUserId!.Value)
            : await walletManagementService
                .GetWalletByNumberAsync(request.TargetWalletNumber)
        ?? throw new ServiceException(ErrorCode.BR_WLT_WalletIsNotExist);

        request.TargetWalletNumber = targetWallet.WalletNumber;

        return await transactionManagementService
                    .CreateTransferTransactionAsync(
                        currentUserWallet.WalletNumber,
                        request.CreateTransactionRequest);
    }
}
