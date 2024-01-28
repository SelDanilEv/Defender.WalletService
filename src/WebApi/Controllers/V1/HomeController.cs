using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Defender.ServiceTemplate.Application.Modules.Home.Queries;
using Defender.Common.Attributes;
using Defender.Common.Models;
using Defender.Common.Enums;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Defender.Common.Interfaces;

namespace Defender.ServiceTemplate.WebUI.Controllers.V1;

public class HomeController : BaseApiController
{
    private readonly IAccountAccessor _accountAccessor;

    public HomeController(
        IAccountAccessor accountAccessor,
        IMediator mediator,
        IMapper mapper)
        : base(mediator, mapper)
    {
        _accountAccessor = accountAccessor;
    }

    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<object> HealthCheckAsync()
    {
        return new { Status = "Healthy" };
    }

    [HttpGet("authorization/check")]
    [Auth(Roles.User)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<object> AuthorizationCheckAsync()
    {
        return new { IsAuthorized = true, Role = _accountAccessor.AccountInfo.GetHighestRole() };
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

        return await ProcessApiCallWithoutMappingAsync<GetConfigurationQuery, Dictionary<string, string>>(query);
    }
}
