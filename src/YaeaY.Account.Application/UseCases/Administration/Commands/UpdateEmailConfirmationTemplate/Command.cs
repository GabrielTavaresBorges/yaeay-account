using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;
namespace YaeaY.Account.Application.UseCases.Administration.Commands.UpdateEmailConfirmationTemplate;
public sealed record Command(Guid AdministratorId, string Subject, string BodyHtml, string Justification) : IRequest<Result<Response>>;
public sealed record Response(DateTimeOffset UpdatedAt);
