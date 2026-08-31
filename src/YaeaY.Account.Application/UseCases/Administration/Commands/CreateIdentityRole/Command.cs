using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;
namespace YaeaY.Account.Application.UseCases.Administration.Commands.CreateIdentityRole;
public sealed record Command(Guid AdministratorId, string Name, string Justification) : IRequest<Result<Response>>;
public sealed record Response(string Name);
