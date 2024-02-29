using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Module.Commands;

public record StartRechargeTransactionCommand : IRequest<Transaction>
{
    public int TargetWalletNumber { get; set; }
    public int Amount { get; set; }
    public Currency Currency { get; set; }
};

public sealed class StartRechargeTransactionCommandValidator :
    AbstractValidator<StartRechargeTransactionCommand>
{
    public StartRechargeTransactionCommandValidator()
    {
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
