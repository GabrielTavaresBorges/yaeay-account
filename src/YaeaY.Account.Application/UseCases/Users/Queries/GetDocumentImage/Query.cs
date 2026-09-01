using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.Users.Queries.GetDocumentImage;

public sealed record Query(Guid UserId, Guid ImageId) : IRequest<Result<Response>>;
