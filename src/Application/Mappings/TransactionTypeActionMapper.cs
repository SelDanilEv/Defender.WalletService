using Defender.Common.DB.SharedStorage.Enums;
using Defender.WalletService.Domain.Entities.Transactions;
using MongoDB.Driver;

namespace Defender.WalletService.Application.Mappings;
internal class TransactionTypeActionMapper :
    Dictionary<TransactionType, Func<Transaction, IClientSessionHandle, Task>>
{
}
