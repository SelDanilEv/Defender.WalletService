using Defender.Common.Configuration.Options;
using Defender.Common.DB.Model;
using Defender.Common.DB.Repositories;
using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.WalletService.Application.Common.Interfaces.Repositories;
using Defender.WalletService.Domain.Entities.Wallets;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Defender.WalletService.Infrastructure.Repositories.DomainModels;

public class WalletRepository(
        IOptions<MongoDbOptions> mongoOption) 
    : BaseMongoRepository<Wallet>(mongoOption.Value), IWalletRepository
{
    public async Task<Wallet> GetWalletByUserIdAsync(Guid userId)
    {
        return await GetItemAsync(userId);
    }

    public async Task<Wallet> GetWalletByNumberAsync(int walletNumber)
    {
        var filter = FindModelRequest<Wallet>.Init(x => x.WalletNumber, walletNumber);

        return await GetItemAsync(filter);
    }

    public async Task<Wallet> CreateNewWalletAsync(Wallet wallet)
    {
        wallet.WalletNumber = await GenerateUniqueWalletNumber();

        return await AddItemAsync(wallet);
    }

    public async Task<bool> IsWalletExistAsync(int walletNumber)
    {
        var request = FindModelRequest<Wallet>
            .Init(x => x.WalletNumber, walletNumber);

        return await CountItemsAsync(request) > 0;
    }

    public async Task<Wallet> UpdateCurrencyAccountsAsync(
        Guid walletId,
        HashSet<CurrencyAccount> currencyAccounts,
        IClientSessionHandle? clientSessionHandle = null)
    {
        var request = UpdateModelRequest<Wallet>
            .Init(walletId)
            .Set(
                x => x.CurrencyAccounts,
                currencyAccounts);

        return await UpdateItemAsync(request, clientSessionHandle);
    }

    public async Task<IClientSessionHandle> StartSessionAsync()
    {
        var sessionOptions = new ClientSessionOptions
        {
            CausalConsistency = true
        };

        return await _client!.StartSessionAsync(sessionOptions);
    }

    private async Task<int> GenerateUniqueWalletNumber()
    {
        var availableNumbers = Enumerable.Range(10000000, 10000000).ToList();

        var walletNumbers = await _mongoCollection
                              .AsQueryable()
                              .Select(wallet => wallet.WalletNumber)
                              .ToListAsync();

        availableNumbers = availableNumbers.Except(walletNumbers).ToList();

        if (!availableNumbers.Any())
            throw new ServiceException(ErrorCode.BR_WLT_NoAvailableWalletNumbers);

        var randomNumberIndex = new Random().Next(0, availableNumbers.Count - 1);

        return availableNumbers[randomNumberIndex];
    }
}
