using Defender.Common.Errors;
using Defender.Common.Extension;
using Defender.WalletService.Application.Common.Interfaces.Services;
using Defender.WalletService.Domain.Entities.Transactions;
using FluentValidation;
using MediatR;

namespace Defender.WalletService.Application.Modules.Transactions.Commands;

public record CancelTransactionCommand : IRequest<Transaction>
{
    public string? TransactionId { get; set; }
};

public sealed class StartCancelationTransactionCommandValidator :
    AbstractValidator<CancelTransactionCommand>
{
    public StartCancelationTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotNull()
            .NotEmpty()
            .WithMessage(ErrorCode.VL_WLT_EmptyWalletNumber);
    }
}

public sealed class StartCancelationTransactionCommandHandler(
        ITransactionManagementService transactionManagementService) :
    IRequestHandler<CancelTransactionCommand, Transaction>
{
    public async Task<Transaction> Handle(
        CancelTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var transaction = await transactionManagementService
            .CancelTransactionAsync(request.TransactionId);

        return transaction;
    }
}
