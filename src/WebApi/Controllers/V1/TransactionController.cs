using System.Threading.Tasks;
using AutoMapper;
using Defender.Common.Attributes;
using Defender.Common.Consts;
using Defender.Common.DB.Pagination;
using Defender.WalletService.Application.DTOs;
using Defender.WalletService.Application.Modules.Transactions.Commands;
using Defender.WalletService.Application.Modules.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.V1;

public class TransactionController(IMediator mediator, IMapper mapper)
    : BaseApiController(mediator, mapper)
{
    [HttpGet("status")]
    [Auth(Roles.User)]
    [ProducesResponseType(typeof(AnonymousTransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AnonymousTransactionDto> GetUserTransactionsAsync(
        [FromQuery] GetUserTransactionByTransactionIdQuery query)
    {
        return await ProcessApiCallAsync<
            GetUserTransactionByTransactionIdQuery,
            AnonymousTransactionDto>(query);
    }

    [HttpGet("history")]
    [Auth(Roles.User)]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<PagedResult<TransactionDto>> GetUserTransactionsAsync(
        [FromQuery] GetTransactionHistoryQuery query)
    {
        return await ProcessApiCallAsync<
            GetTransactionHistoryQuery,
            PagedResult<TransactionDto>>(query);
    }

    [HttpPost("payment")]
    [Auth(Roles.User)]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<TransactionDto> CreatePaymentTransactionAsync(
        [FromBody] StartPaymentTransactionCommand command)
    {
        return await ProcessApiCallAsync<
            StartPaymentTransactionCommand,
            TransactionDto>(command);
    }

    [HttpPost("recharge")]
    [Auth(Roles.SuperAdmin)]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<TransactionDto> CreateRechargeTransactionAsync(
        [FromBody] StartRechargeTransactionCommand command)
    {
        return await ProcessApiCallAsync<StartRechargeTransactionCommand, TransactionDto>(command);
    }

    [HttpPost("transfer")]
    [Auth(Roles.User)]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<TransactionDto> CreateTransferTransactionAsync(
        [FromBody] StartTransferTransactionCommand command)
    {
        return await ProcessApiCallAsync<StartTransferTransactionCommand, TransactionDto>(command);
    }

    [HttpDelete("cancel")]
    [Auth(Roles.SuperAdmin)]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<TransactionDto> CancelTransactionAsync(
        [FromBody] CancelTransactionCommand command)
    {
        return await ProcessApiCallAsync<CancelTransactionCommand, TransactionDto>(command);
    }

}
