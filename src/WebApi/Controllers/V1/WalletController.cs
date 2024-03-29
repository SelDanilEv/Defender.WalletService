﻿using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Defender.Common.Attributes;
using Defender.Common.Models;
using Defender.WalletService.Application.Modules.Module.Commands;
using Defender.WalletService.Application.DTOs;

namespace Defender.WalletService.WebUI.Controllers.V1;

public class WalletController : BaseApiController
{
    public WalletController(IMediator mediator, IMapper mapper) : base(mediator, mapper)
    {
    }

    [HttpGet("get-or-create")]
    [Auth(Roles.User)]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<WalletDto> GetOrCreateWalletAsync(
        [FromQuery] GetOrCreateWalletCommand command)
    {
        return await ProcessApiCallAsync<GetOrCreateWalletCommand, WalletDto>(command);
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
