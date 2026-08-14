using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Events.Users;

namespace YaeaY.Account.Application.UseCases.EmailConfirmations.Commands.IssueInitialToken;

public sealed record Command(UserRegisteredDomainEvent DomainEvent) : IRequest<Result<Response>>;
