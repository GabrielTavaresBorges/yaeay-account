using MediatR;
using YaeaY.Account.Application.Services.Administration.Interfaces;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Entities.AggregateRoots.Administration;
using YaeaY.Account.Domain.Repositories.Administration;

namespace YaeaY.Account.Application.UseCases.Administration.Commands.CreateIdentityRole;

public sealed class Handler(IAdministrationConfigurationService configuration, IAdministrationAuditRepository audit, IUnitOfWork unitOfWork, TimeProvider timeProvider) : IRequestHandler<Command, Result<Response>>
{
    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<Response>.Failure(new("administration.role.name.required", "Nome da função é obrigatório.", ErrorCategory.Validation, ErrorRule.Required));
        var result = await configuration.CreateRoleAsync(command.Name, cancellationToken);
        if (!result.Succeeded)
            return Result<Response>.Failure(new("administration.role.create.failed", result.Error ?? "Não foi possível criar a função.", ErrorCategory.Validation, ErrorRule.InvalidValue));
        await audit.AddAsync(AdministrationAuditEntry.Create(command.AdministratorId, null, $"IdentityRole.Created:{command.Name.Trim()}", command.Justification, timeProvider.GetUtcNow()), cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result<Response>.Success(new(command.Name.Trim()));
    }
}
