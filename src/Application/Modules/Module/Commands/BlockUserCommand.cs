using Defender.Common.Errors;
using Defender.Common.Interfaces;
using Defender.ServiceTemplate.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Defender.ServiceTemplate.Application.Modules.Module.Commands;

public record ModuleCommand : IRequest<Unit>
{
    public bool DoModule { get; set; } = true;
};

public sealed class ModuleCommandValidator : AbstractValidator<ModuleCommand>
{
    public ModuleCommandValidator()
    {
        RuleFor(s => s.DoModule)
                  .NotEmpty().WithMessage(ErrorCodeHelper.GetErrorCode(ErrorCode.VL_InvalidRequest));
    }
}

public sealed class ModuleCommandHandler : IRequestHandler<ModuleCommand, Unit>
{
    private readonly IAccountAccessor _accountAccessor;
    private readonly IService _accountManagementService;

    public ModuleCommandHandler(
        IAccountAccessor accountAccessor,
        IService accountManagementService
        )
    {
        _accountAccessor = accountAccessor;
        _accountManagementService = accountManagementService;
    }

    public async Task<Unit> Handle(ModuleCommand request, CancellationToken cancellationToken)
    {
        return Unit.Value;
    }
}
