using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Module.Commands;

public record StartPaymentTransactionCommand : IRequest<Transaction>
{
    public int? TargetWalletNumber { get; set; } = null;
    public int Amount { get; set; }
    public Currency Currency { get; set; }
};

public sealed class StartPaymentTransactionCommandValidator :
    AbstractValidator<StartPaymentTransactionCommand>
{
    public StartPaymentTransactionCommandValidator()
    {
    }
}

public sealed class StartPaymentTransactionCommandHandler :
    IRequestHandler<StartPaymentTransactionCommand, Transaction>
{
    private readonly ICurrentAccountAccessor _currentAccountAccessor;
    private readonly IAuthorizationCheckingService _authorizationCheckingService;
    private readonly ITransactionManagementService _transactionManagementService;
    private readonly IWalletManagementService _walletManagementService;

    public StartPaymentTransactionCommandHandler(
        IAuthorizationCheckingService authorizationCheckingService,
        ICurrentAccountAccessor currentAccountAccessor,
        ITransactionManagementService transactionManagementService,
        IWalletManagementService walletManagementService
        )
    {
        _authorizationCheckingService = authorizationCheckingService;
        _currentAccountAccessor = currentAccountAccessor;
        _transactionManagementService = transactionManagementService;
        _walletManagementService = walletManagementService;
    }

    public async Task<Transaction> Handle(
        StartPaymentTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var transaction = default(Transaction);

        var userId = _currentAccountAccessor.GetAccountId();

        var currentUserWallet = await _walletManagementService
            .GetWalletByUserIdAsync(userId);

        if (request.TargetWalletNumber.HasValue
            && currentUserWallet.WalletNumber != request.TargetWalletNumber.Value)
        {
            var targetWallet = await _walletManagementService
                .GetWalletByNumberAsync(request.TargetWalletNumber.Value);

            transaction = await _authorizationCheckingService
                .RunWithAuthAsync(
                    targetWallet.Id,
                    async () => await _transactionManagementService
                        .CreatePaymentTransactionAsync(
                            targetWallet.WalletNumber,
                            request.Amount,
                            request.Currency));
        }
        else
        {
            transaction = await _transactionManagementService
                .CreatePaymentTransactionAsync(
                    currentUserWallet.WalletNumber,
                    request.Amount,
                    request.Currency);
        }


        return transaction;
    }
}
