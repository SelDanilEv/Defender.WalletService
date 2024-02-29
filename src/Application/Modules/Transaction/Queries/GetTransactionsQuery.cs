using Defender.Common.DB.Pagination;
using Defender.Common.Interfaces;
using Defender.WalletService.Application.Common.Interfaces;
using Defender.WalletService.Domain.Entities.Transactions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Defender.NotificationService.Application.Modules.Monitoring.Queries;

public record GetTransactionsQuery : PaginationRequest, IRequest<PagedResult<Transaction>>
{
    public Guid? WalletId { get; set; }
};

public sealed class GetTransactionsQueryValidator : AbstractValidator<GetTransactionsQuery>
{
    public GetTransactionsQueryValidator()
    {
    }
}

public sealed class GetTransactionsQueryHandler :
    IRequestHandler<GetTransactionsQuery, PagedResult<Transaction>>
{
    private readonly ICurrentAccountAccessor _currentAccountAccessor;
    private readonly IAuthorizationCheckingService _authorizationCheckingService;
    private readonly ITransactionManagementService _transactionManagementService;
    private readonly IWalletManagementService _walletManagementService;

    public GetTransactionsQueryHandler(
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

    public async Task<PagedResult<Transaction>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        return request.WalletId.HasValue ?
            await _authorizationCheckingService.RunWithAuthAsync(
                request.WalletId.Value,
                async () => await GetTransactions(request, request.WalletId.Value)
            ) :
            await GetTransactions(request, _currentAccountAccessor.GetAccountId());
    }

    private async Task<PagedResult<Transaction>> GetTransactions(
        PaginationRequest request,
        Guid userId)
    {
        var wallet = await _walletManagementService.GetWalletByUserIdAsync(userId);

        return await _transactionManagementService.GetTransactionsByWalletNumberAsync(
            request,
            wallet.WalletNumber);
    }
}
