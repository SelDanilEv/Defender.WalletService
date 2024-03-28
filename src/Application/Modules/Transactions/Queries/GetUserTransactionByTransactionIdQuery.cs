using Defender.Common.DB.Pagination;
using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Domain.Entities.Transactions;
using FluentValidation;
using MediatR;

namespace Defender.NotificationService.Application.Modules.Monitoring.Queries;

public record GetUserTransactionByTransactionIdQuery : PaginationRequest, IRequest<Transaction>
{
    //public Guid UserId { get; set; }
    public string? TransactionId { get; set; }
};

public sealed class GetUserTransactionByTransactionIdQueryValidator : AbstractValidator<GetUserTransactionByTransactionIdQuery>
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

public sealed class GetUserTransactionByTransactionIdQueryHandler :
    IRequestHandler<GetUserTransactionByTransactionIdQuery, Transaction>
{
    private readonly ICurrentAccountAccessor _currentAccountAccessor;
    private readonly IAuthorizationCheckingService _authorizationCheckingService;
    private readonly ITransactionManagementService _transactionManagementService;
    private readonly IWalletManagementService _walletManagementService;

    public GetUserTransactionByTransactionIdQueryHandler(
        IAuthorizationCheckingService authorizationCheckingService,
        ICurrentAccountAccessor currentAccountAccessor,
        ITransactionManagementService transactionManagementService,
        IWalletManagementService walletManagementService
        )
    {
        _authorizationCheckingService = authorizationCheckingService;
        _currentAccountAccessor = currentAccountAccessor;
        _transactionManagementService = transactionManagementService;
        _walletManagementService = walletManagementService;
    }

    public async Task<Transaction> Handle(
        GetUserTransactionByTransactionIdQuery request,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactionManagementService
            .GetTransactionByTransactionIdAsync(request.TransactionId);

        if(transaction == null)
        {
            throw new NotFoundException();
        }

        return transaction;
    }
}
