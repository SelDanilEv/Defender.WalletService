using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Domain.Entities.Wallets;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Module.Commands;

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

public sealed class GetOrCreateWalletCommandHandler : IRequestHandler<GetOrCreateWalletCommand, Wallet>
{
    private readonly ICurrentAccountAccessor _currentAccountAccessor;
    private readonly IWalletManagementService _walletManagementService;

    public GetOrCreateWalletCommandHandler(
        ICurrentAccountAccessor currentAccountAccessor,
        IWalletManagementService walletManagementService
        )
    {
        _currentAccountAccessor = currentAccountAccessor;
        _walletManagementService = walletManagementService;
    }

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
