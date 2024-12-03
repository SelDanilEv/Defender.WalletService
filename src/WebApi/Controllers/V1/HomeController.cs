using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Defender.Common.Attributes;
using Defender.Common.Consts;
using Defender.Common.DTOs;
using Defender.Common.Enums;
using Defender.Common.Modules.Home.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.V1;

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
