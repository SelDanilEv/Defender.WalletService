using Defender.Common.DB.Pagination;
using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Domain.Entities.Transactions;
using FluentValidation;
using MediatR;

namespace Defender.NotificationService.Application.Modules.Monitoring.Queries;

public record GetTransactionHistoryQuery
    : PaginationRequest, IRequest<PagedResult<Transaction>>
{
    public Guid? WalletId { get; set; }
};

public sealed class GetTransactionsQueryValidator
    : AbstractValidator<GetTransactionHistoryQuery>
{
    public GetTransactionsQueryValidator()
    {
    }
}

public sealed class GetTransactionsQueryHandler(
        IAuthorizationCheckingService authorizationCheckingService,
        ICurrentAccountAccessor currentAccountAccessor,
        ITransactionManagementService transactionManagementService,
        IWalletManagementService walletManagementService
        )
    : IRequestHandler<GetTransactionHistoryQuery, PagedResult<Transaction>>
{
    public async Task<PagedResult<Transaction>> Handle(
        GetTransactionHistoryQuery request, 
        CancellationToken cancellationToken)
    {
        return request.WalletId.HasValue ?
            await authorizationCheckingService.RunWithAuthAsync(
                request.WalletId.Value,
                async () => await GetTransactions(request, request.WalletId.Value)
            ) :
            await GetTransactions(request, currentAccountAccessor.GetAccountId());
    }

    private async Task<PagedResult<Transaction>> GetTransactions(
        PaginationRequest request,
        Guid userId)
    {
        var wallet = await walletManagementService.GetWalletByUserIdAsync(userId);

        return await transactionManagementService.GetTransactionsByWalletNumberAsync(
            request,
            wallet.WalletNumber);
    }
}
