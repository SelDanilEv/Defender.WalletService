using Defender.Common.DB.Model;
using Defender.Common.DB.Pagination;
using Defender.WalletService.Domain.Entities.Transactions;

namespace Defender.WalletService.Application.Common.Interfaces.Repositories;

public interface ITransactionRepository
{
    Task<Transaction> GetTransactionByIdAsync(string transactionId);
    Task<PagedResult<Transaction>> GetTransactionsAsync(
        PaginationRequest paginationRequest,
        int walletNumber);
    Task<Transaction> UpdateTransactionAsync(
        UpdateModelRequest<Transaction> updateRequest);
    Task<Transaction> CreateNewTransactionAsync(Transaction wallet);
    Task<Transaction> GetLastProccedTransaction();
}
