using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.WalletService.Domain.Entities.Transactions;
using FluentValidation;
using Defender.Common.Extension;
using MediatR;
using Defender.WalletService.Application.Common.Interfaces.Services;

namespace Defender.WalletService.Application.Modules.Transactions.Queries;

public record GetUserTransactionByTransactionIdQuery
    : IRequest<Transaction>
{
    //public Guid UserId { get; set; }
    public string? TransactionId { get; set; }
};

public sealed class GetUserTransactionByTransactionIdQueryValidator
    : AbstractValidator<GetUserTransactionByTransactionIdQuery>
{
    public GetUserTransactionByTransactionIdQueryValidator()
    {
        RuleFor(s => s.TransactionId)
                  .NotEmpty()
                  .NotNull()
                  .WithMessage(
            ErrorCodeHelper.GetErrorCode(ErrorCode.VL_WLT_EmptyTransactionId));
    }
}

public sealed class GetUserTransactionByTransactionIdQueryHandler(
    ITransactionManagementService transactionManagementService)
    : IRequestHandler<GetUserTransactionByTransactionIdQuery, Transaction>
{
    public async Task<Transaction> Handle(
        GetUserTransactionByTransactionIdQuery request,
        CancellationToken cancellationToken)
    {
        var transaction = await transactionManagementService
            .GetTransactionByTransactionIdAsync(request.TransactionId);

        if (transaction == null)
        {
            throw new NotFoundException();
        }

        return transaction;
    }
}
