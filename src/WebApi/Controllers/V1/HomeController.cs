using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Defender.Common.Attributes;
using Defender.Common.Consts;
using Defender.Common.Enums;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Defender.Common.Modules.Home.Queries;
using Defender.Common.DTOs;

namespace Defender.WalletService.WebUI.Controllers.V1;

public partial class HomeController(
        IMediator mediator,
        IMapper mapper)
    : BaseApiController(mediator, mapper)
{
    [HttpGet("health")]
    [ProducesResponseType(typeof(HealthCheckDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<HealthCheckDto> HealthCheckAsync()
    {
        var query = new HealthCheckQuery();

        return await ProcessApiCallWithoutMappingAsync<
            HealthCheckQuery,
            HealthCheckDto>(query);
    }

    [HttpGet("authorization/check")]
    [Auth(Roles.User)]
    [ProducesResponseType(typeof(AuthCheckDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthCheckDto> AuthorizationCheckAsync()
    {
        var query = new AuthCheckQuery();

        return await ProcessApiCallWithoutMappingAsync<
            AuthCheckQuery,
            AuthCheckDto>(query);
    }

    [Auth(Roles.SuperAdmin)]
    [HttpGet("configuration")]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<Dictionary<string, string>> GetConfigurationAsync(ConfigurationLevel configurationLevel)
    {
        var query = new GetConfigurationQuery()
        {
            Level = configurationLevel
        };

        return await ProcessApiCallWithoutMappingAsync<
            GetConfigurationQuery,
            Dictionary<string, string>>(query);
    }
}
