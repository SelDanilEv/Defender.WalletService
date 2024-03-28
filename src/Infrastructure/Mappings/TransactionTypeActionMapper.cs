using Defender.WalletService.Domain.Entities.Transactions;
using Defender.WalletService.Domain.Enums;
using MongoDB.Driver;

namespace Defender.WalletService.Infrastructure.Mappings;
internal class TransactionTypeActionMapper :
    Dictionary<TransactionType, Func<Transaction, IClientSessionHandle, Task>>
{
}
