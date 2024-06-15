using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.WalletService.Domain.Entities.Transactions;
using FluentValidation;
using Defender.Common.Extension;
using MediatR;
using Defender.WalletService.Application.Common.Interfaces.Services;

namespace Defender.WalletService.Application.Modules.Transactions.Commands;

public record StartRechargeTransactionCommand : BaseTransactionCommand, IRequest<Transaction>
{
};

public sealed class StartRechargeTransactionCommandValidator :
    AbstractValidator<StartRechargeTransactionCommand>
{
    public StartRechargeTransactionCommandValidator()
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

public sealed class StartRechargeTransactionCommandHandler(
        ITransactionManagementService transactionManagementService,
        IWalletManagementService walletManagementService
        ) :
    IRequestHandler<StartRechargeTransactionCommand, Transaction>
{
    public async Task<Transaction> Handle(
        StartRechargeTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var targetWallet = (request.TargetUserId.HasValue
            ? await walletManagementService
                .GetWalletByUserIdAsync(request.TargetUserId!.Value)
            : await walletManagementService
                .GetWalletByNumberAsync(request.TargetWalletNumber))
        ?? throw new ServiceException(ErrorCode.BR_WLT_WalletIsNotExist);

        request.TargetWalletNumber = targetWallet.WalletNumber;

        return await transactionManagementService
                    .CreateRechargeTransactionAsync(request.CreateTransactionRequest);
    }
}
