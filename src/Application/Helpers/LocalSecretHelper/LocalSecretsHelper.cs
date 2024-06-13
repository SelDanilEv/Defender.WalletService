using Defender.Common.Enums;
using Defender.Common.Helpers;

namespace Defender.WalletService.Application.Helpers.LocalSecretHelper;

public class LocalSecretsHelper
{
    public static async Task<string> GetSecretAsync(Secret secret)
    {
        return await SecretsHelper.GetSecretAsync(secret);
    }

    public static async Task<string> GetSecretAsync(LocalSecret secret)
    {
        return await SecretsHelper.GetSecretAsync(secret.ToString());
    }

    public static string GetSecretSync(Secret secret)
    {
        return SecretsHelper.GetSecretSync(secret);
    }

    public static string GetSecretSync(LocalSecret secret)
    {
        return SecretsHelper.GetSecretSync(secret.ToString());
    }
}
