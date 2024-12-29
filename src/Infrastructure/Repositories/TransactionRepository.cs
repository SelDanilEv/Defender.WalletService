using Defender.Common.Configuration.Options;
using Defender.Common.DB.Model;
using Defender.Common.DB.Pagination;
using Defender.Common.DB.Repositories;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Domain.Entities.Transactions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Defender.WalletService.Infrastructure.Repositories;

public class TransactionRepository
    : BaseMongoRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(IOptions<MongoDbOptions> mongoOption) : base(mongoOption.Value)
    {
        _mongoCollection.Indexes.CreateMany([
            new CreateIndexModel<Transaction>(
                Builders<Transaction>.IndexKeys.Ascending(x => x.TransactionId),
                new CreateIndexOptions { Unique = true, Name = "TransactionId_Index" }),
            new CreateIndexModel<Transaction>(
                Builders<Transaction>.IndexKeys
                    .Ascending(x => x.FromWallet)
                    .Ascending(x => x.ToWallet)
                    .Descending(x => x.UtcTransactionDate),
                new CreateIndexOptions { Name = "User_Transactions_Index" })
        ]);
    }
    
    public async Task<PagedResult<Transaction>> GetTransactionsAsync(
        PaginationRequest paginationRequest,
        int walletNumber)
    {
        var settings = PaginationSettings<Transaction>
            .FromPaginationRequest(paginationRequest);

        var filterRequest = FindModelRequest<Transaction>
            .Init(x => x.FromWallet, walletNumber)
            .Or(x => x.ToWallet, walletNumber)
            .Sort(x => x.UtcTransactionDate, SortType.Desc);

        settings.SetupFindOptions(filterRequest);

        return await GetItemsAsync(settings);
    }

    public async Task<Transaction> GetTransactionByIdAsync(string transactionId)
    {
        var filterRequest = FindModelRequest<Transaction>
            .Init(x => x.TransactionId, transactionId);

        return await GetItemAsync(filterRequest);
    }

    public async Task<Transaction> UpdateTransactionAsync(
        UpdateModelRequest<Transaction> updateRequest)
    {
        return await UpdateItemAsync(updateRequest);
    }

    public async Task<Transaction> CreateNewTransactionAsync(Transaction transaction)
    {
        return await AddItemAsync(transaction);
    }

}


