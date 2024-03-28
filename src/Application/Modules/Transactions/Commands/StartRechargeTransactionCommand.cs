using Defender.Common.Errors;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Domain.Entities.Transactions;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Transactions.Commands;

public record StartRechargeTransactionCommand : BaseTransactionCommand, IRequest<Transaction>
{
    public int TargetWalletNumber { get; set; }
};

public sealed class StartRechargeTransactionCommandValidator :
    AbstractValidator<StartRechargeTransactionCommand>
{
    public StartRechargeTransactionCommandValidator()
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

public sealed class StartRechargeTransactionCommandHandler :
    IRequestHandler<StartRechargeTransactionCommand, Transaction>
{
    private readonly ITransactionManagementService _transactionManagementService;
    private readonly IWalletManagementService _walletManagementService;

    public StartRechargeTransactionCommandHandler(
        ITransactionManagementService transactionManagementService,
        IWalletManagementService walletManagementService
        )
    {
        _transactionManagementService = transactionManagementService;
        _walletManagementService = walletManagementService;
    }

    public async Task<Transaction> Handle(
        StartRechargeTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var targetWallet = await _walletManagementService
            .GetWalletByNumberAsync(request.TargetWalletNumber);

        var transaction = await _transactionManagementService
                    .CreateRechargeTransactionAsync(
                        targetWallet.WalletNumber,
                        request.Amount,
                        request.Currency);

        return transaction;
    }
}
