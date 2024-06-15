using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Domain.Entities.Wallets;
using Defender.WalletService.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Wallets.Commands;

public record SetDefaultCurrencyAccountCommand : IRequest<Wallet>
{
    public Currency Currency { get; set; }
};

public sealed class SetDefaultCurrencyAccountCommandValidator :
    AbstractValidator<SetDefaultCurrencyAccountCommand>
{
    public SetDefaultCurrencyAccountCommandValidator()
    {
    }
}

public sealed class SetDefaultCurrencyAccountCommandHandler(
        ICurrentAccountAccessor currentAccountAccessor,
        IWalletManagementService walletManagementService)
    : IRequestHandler<SetDefaultCurrencyAccountCommand, Wallet>
{
    public async Task<Wallet> Handle(
        SetDefaultCurrencyAccountCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentAccountAccessor.GetAccountId();

        var wallet = await walletManagementService
            .SetDefaultCurrencyAccountAsync(userId, request.Currency);

        return wallet;
    }
}
