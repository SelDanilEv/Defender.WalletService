using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Domain.Entities.Wallets;
using Defender.WalletService.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Module.Commands;

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

public sealed class SetDefaultCurrencyAccountCommandHandler : 
    IRequestHandler<SetDefaultCurrencyAccountCommand, Wallet>
{
    private readonly ICurrentAccountAccessor _currentAccountAccessor;
    private readonly IWalletManagementService _walletManagementService;

    public SetDefaultCurrencyAccountCommandHandler(
        ICurrentAccountAccessor currentAccountAccessor,
        IWalletManagementService walletManagementService
        )
    {
        _currentAccountAccessor = currentAccountAccessor;
        _walletManagementService = walletManagementService;
    }

    public async Task<Wallet> Handle(
        SetDefaultCurrencyAccountCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentAccountAccessor.GetAccountId();

        var wallet = await _walletManagementService
            .SetDefaultCurrencyAccountAsync(userId, request.Currency);

        return wallet;
    }
}
