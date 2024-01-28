using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Defender.ServiceTemplate.WebUI.Controllers;

[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    protected readonly IMediator Mediator;
    protected readonly IMapper Mapper;

    public BaseApiController(IMediator mediator, IMapper mapper)
    {
        Mediator = mediator;
        Mapper = mapper;
    }

    protected async Task<TResult> ProcessApiCallAsync<TRequest, TResult>(TRequest request)
    {
        var response = await Mediator.Send(request);

        var result = Mapper.Map<TResult>(response);

        return result;
    }

    protected async Task ProcessApiCallAsync<TRequest>(TRequest request)
    {
        await Mediator.Send(request);
    }

    protected async Task<TResult> ProcessApiCallWithoutMappingAsync<TRequest, TResult>(TRequest request)
    {
        var response = await Mediator.Send(request);

        return (TResult)response;
    }
}
