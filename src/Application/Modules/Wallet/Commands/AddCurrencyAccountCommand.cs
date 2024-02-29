using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Domain.Entities.Wallets;
using Defender.WalletService.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Module.Commands;

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

public sealed class AddCurrencyAccountCommandHandler : IRequestHandler<AddCurrencyAccountCommand, Wallet>
{
    private readonly ICurrentAccountAccessor _currentAccountAccessor;
    private readonly IWalletManagementService _walletManagementService;

    public AddCurrencyAccountCommandHandler(
        ICurrentAccountAccessor currentAccountAccessor,
        IWalletManagementService walletManagementService
        )
    {
        _currentAccountAccessor = currentAccountAccessor;
        _walletManagementService = walletManagementService;
    }

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
