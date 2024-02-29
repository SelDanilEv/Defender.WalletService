using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Defender.WalletService.Application.Modules.Home.Queries;
using Defender.Common.Attributes;
using Defender.Common.Models;
using Defender.Common.Enums;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Defender.Common.Interfaces;
using Defender.Common.Helpers;

namespace Defender.WalletService.WebUI.Controllers.V1;

public class HomeController : BaseApiController
{

    private readonly ICurrentAccountAccessor _accountAccessor;

    public HomeController(
        ICurrentAccountAccessor accountAccessor,
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
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<string> AuthorizationCheckAsync()
    {
        var userRoles = _accountAccessor.GetRoles();

        return RolesHelper.GetHighestRole(userRoles);
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
