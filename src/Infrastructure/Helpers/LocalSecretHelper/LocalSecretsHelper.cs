using Defender.Common.Enums;
using Defender.Common.Helpers;

namespace Defender.ServiceTemplate.Infrastructure.Helpers.LocalSecretHelper;

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
}
