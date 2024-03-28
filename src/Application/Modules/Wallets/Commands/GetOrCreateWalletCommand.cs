using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Domain.Entities.Wallets;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Wallets.Commands;

public record GetOrCreateWalletCommand : IRequest<Wallet>
{
    //public Guid? UserId { get; set; }
};

public sealed class GetOrCreateWalletCommandValidator : AbstractValidator<GetOrCreateWalletCommand>
{
    public GetOrCreateWalletCommandValidator()
    {
    }
}

public sealed class GetOrCreateWalletCommandHandler(
        ICurrentAccountAccessor _currentAccountAccessor,
        IWalletManagementService _walletManagementService)
    : IRequestHandler<GetOrCreateWalletCommand, Wallet>
{
    public async Task<Wallet> Handle(
        GetOrCreateWalletCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentAccountAccessor.GetAccountId();

        var wallet = await _walletManagementService.GetWalletByUserIdAsync(userId);

        if (wallet == null)
        {
            wallet = await _walletManagementService.CreateNewWalletAsync();
        }

        return wallet;
    }
}
