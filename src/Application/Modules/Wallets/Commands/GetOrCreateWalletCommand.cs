using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Domain.Entities.Wallets;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Wallets.Commands;

public record GetOrCreateWalletCommand : IRequest<Wallet>
{
    public Guid? UserId { get; set; }
};

public sealed class GetOrCreateWalletCommandValidator : AbstractValidator<GetOrCreateWalletCommand>
{
    public GetOrCreateWalletCommandValidator()
    {
    }
}

public sealed class GetOrCreateWalletCommandHandler(
        IAuthorizationCheckingService authorizationCheckingService,
        ICurrentAccountAccessor currentAccountAccessor,
        IWalletManagementService walletManagementService)
    : IRequestHandler<GetOrCreateWalletCommand, Wallet>
{
    public async Task<Wallet> Handle(
        GetOrCreateWalletCommand request,
        CancellationToken cancellationToken)
    {
        var targetUserId = request.UserId ?? currentAccountAccessor.GetAccountId();

        var wallet = await authorizationCheckingService.ExecuteWithAuthCheckAsync(targetUserId,
            async () => await GetOrCreateWalletAsync(targetUserId));

        return wallet;
    }


    private async Task<Wallet> GetOrCreateWalletAsync(Guid userId)
    {
        var wallet = await walletManagementService.GetWalletByUserIdAsync(userId);

        if (wallet == null)
        {
            wallet = await walletManagementService.CreateNewWalletAsync(userId);
        }

        return wallet;
    }
}
