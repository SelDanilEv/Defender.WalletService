using Defender.WalletService.Domain.Enums;

namespace Defender.WalletService.Application.DTOs;

public class PublicWalletInfoDto()
{
    public Guid OwnerId { get; set; }
    public int WalletNumber { get; set; }
    public List<Currency>? Currencies { get; set; }
}
