using System.Collections;
using Defender.Common.Enums;
using Defender.Common.Helpers;
using MediatR;

namespace Defender.ServiceTemplate.Application.Modules.Home.Queries;

public record GetConfigurationQuery : IRequest<Dictionary<string, string>>
{
    public ConfigurationLevel Level { get; set; } = ConfigurationLevel.All;
};

public class GetConfigurationQueryHandler : IRequestHandler<GetConfigurationQuery, Dictionary<string, string>>
{
    public async Task<Dictionary<string, string>> Handle(GetConfigurationQuery request, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, string>();

        switch (request.Level)
        {
            case ConfigurationLevel.All:
                var allProcessEnvVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
                foreach (DictionaryEntry envVariable in allProcessEnvVariables)
                {
                    result.TryAdd(envVariable.Key.ToString(), envVariable.Value.ToString());
                }

                var allUserEnvVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);
                foreach (DictionaryEntry envVariable in allUserEnvVariables)
                {
                    result.TryAdd(envVariable.Key.ToString(), envVariable.Value.ToString());
                }

                var allMachineEnvVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
                foreach (DictionaryEntry envVariable in allMachineEnvVariables)
                {
                    result.TryAdd(envVariable.Key.ToString(), envVariable.Value.ToString());
                }
                break;
            case ConfigurationLevel.Admin:
                foreach (Secret secret
                    in (Secret[])Enum.GetValues(typeof(Secret)))
                {
                    result.Add(secret.ToString(), await SecretsHelper.GetSecretAsync(secret));
                }
                break;
        }

        return result;
    }

}
