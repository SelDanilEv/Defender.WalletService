using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Domain.Entities.Wallets;
using Defender.WalletService.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Wallets.Commands;

public record AddCurrencyAccountCommand : IRequest<Wallet>
{
    //public Guid? UserId { get; set; }
    public Currency Currency { get; set; }
    public bool IsDefault { get; set; } = false;
};

public sealed class AddCurrencyAccountCommandValidator : AbstractValidator<AddCurrencyAccountCommand>
{
    public AddCurrencyAccountCommandValidator()
    {
    }
}

public sealed class AddCurrencyAccountCommandHandler(
        ICurrentAccountAccessor _currentAccountAccessor,
        IWalletManagementService _walletManagementService)
    : IRequestHandler<AddCurrencyAccountCommand, Wallet>
{
    public async Task<Wallet> Handle(
        AddCurrencyAccountCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentAccountAccessor.GetAccountId();

        var wallet = await _walletManagementService
            .AddCurrencyAccountAsync(userId, request.Currency, request.IsDefault);

        return wallet;
    }
}
