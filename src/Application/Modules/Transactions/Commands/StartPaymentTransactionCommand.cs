using Defender.Common.Errors;
using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Domain.Entities.Transactions;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Transactions.Commands;

public record StartPaymentTransactionCommand : BaseTransactionCommand, IRequest<Transaction>
{
    public int? TargetWalletNumber { get; set; } = null;
};

public sealed class StartPaymentTransactionCommandValidator :
    AbstractValidator<StartPaymentTransactionCommand>
{
    public StartPaymentTransactionCommandValidator()
    {
        RuleFor(x => x.TargetWalletNumber)
            .NotEmpty()
            .WithMessage(ErrorCodeHelper.GetErrorCode(
                ErrorCode.VL_WLT_EmptyWalletNumber))
            .InclusiveBetween(10000000, 99999999)
            .WithMessage(ErrorCodeHelper.GetErrorCode(
                ErrorCode.VL_WLT_InvalidWalletNumber));

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage(ErrorCodeHelper.GetErrorCode(
                ErrorCode.VL_WLT_TransferAmountMustBePositive));
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
        var transaction = default(Transaction);

        var userId = currentAccountAccessor.GetAccountId();

        var currentUserWallet = await walletManagementService
            .GetWalletByUserIdAsync(userId);

        if (request.TargetWalletNumber.HasValue
            && currentUserWallet.WalletNumber != request.TargetWalletNumber.Value)
        {
            var targetWallet = await walletManagementService
                .GetWalletByNumberAsync(request.TargetWalletNumber.Value);

            transaction = await authorizationCheckingService
                .ExecuteWithAuthCheckAsync(
                    targetWallet.Id,
                    async () => await transactionManagementService
                        .CreatePaymentTransactionAsync(
                            targetWallet.WalletNumber,
                            request.Amount,
                            request.Currency));
        }
        else
        {
            transaction = await transactionManagementService
                .CreatePaymentTransactionAsync(
                    currentUserWallet.WalletNumber,
                    request.Amount,
                    request.Currency);
        }


        return transaction;
    }
}
