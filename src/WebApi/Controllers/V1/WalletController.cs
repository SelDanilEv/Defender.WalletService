using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Defender.Common.Attributes;
using Defender.Common.Consts;
using Defender.WalletService.Application.DTOs;
using Defender.WalletService.Application.Modules.Wallets.Commands;
using Defender.WalletService.Application.Modules.Wallets.Queries;

namespace Defender.WalletService.WebApi.Controllers.V1;

public class WalletController(IMediator mediator, IMapper mapper) : BaseApiController(mediator, mapper)
{
    [HttpGet("get-or-create")]
    [Auth(Roles.User)]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<WalletDto> GetOrCreateWalletAsync(
        [FromQuery] GetOrCreateWalletCommand command)
    {
        return await ProcessApiCallAsync<GetOrCreateWalletCommand, WalletDto>(command);
    }

    [HttpGet("public/info-by-number")]
    [Auth(Roles.User)]
    [ProducesResponseType(typeof(PublicWalletInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<PublicWalletInfoDto> GetPublicWalletInfoByNumberAsync(
        [FromQuery] GetWalletInfoByNumberQuery command)
    {
        return await ProcessApiCallAsync<GetWalletInfoByNumberQuery, PublicWalletInfoDto>(command);
    }

    [HttpGet("private/info-by-number")]
    [Auth(Roles.Admin)]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<WalletDto> GetPrivateWalletInfoByNumberAsync(
        [FromQuery] GetWalletInfoByNumberQuery command)
    {
        return await ProcessApiCallAsync<GetWalletInfoByNumberQuery, WalletDto>(command);
    }

    [HttpPost("account/create")]
    [Auth(Roles.User)]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<WalletDto> AddCurrencyAccountAsync(
        [FromBody] AddCurrencyAccountCommand command)
    {
        return await ProcessApiCallAsync<AddCurrencyAccountCommand, WalletDto>(command);
    }

    [HttpPost("account/set-default")]
    [Auth(Roles.User)]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<WalletDto> SetDefaultCurrencyAccountAsync(
        [FromBody] SetDefaultCurrencyAccountCommand command)
    {
        return await ProcessApiCallAsync<SetDefaultCurrencyAccountCommand, WalletDto>(command);
    }

}
