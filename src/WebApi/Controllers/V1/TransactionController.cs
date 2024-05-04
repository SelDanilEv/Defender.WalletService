using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Defender.Common.Attributes;
using Defender.WalletService.Application.DTOs;
using Defender.NotificationService.Application.Modules.Monitoring.Queries;
using Defender.Common.DB.Pagination;
using Defender.WalletService.Application.Modules.Transactions.Commands;
using Defender.Common.Consts;

namespace Defender.WalletService.WebUI.Controllers.V1;

public class TransactionController : BaseApiController
{
    public TransactionController(IMediator mediator, IMapper mapper) : base(mediator, mapper)
    {
    }

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

}
