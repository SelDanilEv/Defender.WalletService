using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.WalletService.Domain.Entities.Wallets;
using FluentValidation;
using Defender.Common.Extension;
using MediatR;
using Defender.WalletService.Application.Common.Interfaces.Services;

namespace Defender.WalletService.Application.Modules.Wallets.Queries;

public record GetWalletInfoByNumberQuery : IRequest<Wallet>
{
    public int WalletNumber { get; set; }
};

public sealed class GetWalletInfoByNumberQueryValidator : AbstractValidator<GetWalletInfoByNumberQuery>
{
    public GetWalletInfoByNumberQueryValidator()
    {
        RuleFor(x => x.WalletNumber)
            .NotNull()
            .WithMessage(ErrorCode.VL_WLT_EmptyWalletNumber)
            .InclusiveBetween(10000000, 99999999)
            .WithMessage(ErrorCode.VL_WLT_InvalidWalletNumber);
    }
}

public sealed class GetWalletInfoByNumberQueryHandler(
    IWalletManagementService _walletManagementService)
    : IRequestHandler<GetWalletInfoByNumberQuery, Wallet>
{
    public async Task<Wallet> Handle(
        GetWalletInfoByNumberQuery request,
        CancellationToken cancellationToken)
    {
        var wallet = await _walletManagementService.GetWalletByNumberAsync(request.WalletNumber);

        if (wallet == null)
        {
            throw new ServiceException(ErrorCode.BR_WLT_WalletIsNotExist);
        }

        return wallet;
    }
}
