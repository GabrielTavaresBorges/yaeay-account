using MediatR;
using YaeaY.Account.Application.Services.ReadModels.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.Users;

namespace YaeaY.Account.Application.UseCases.Users.Queries.GetMyData;

public sealed class Handler(IMyDataReader reader)
    : IRequestHandler<Query, Result<Response>>
{
    public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
    {
        var response = await reader.GetAsync(query.UserId, cancellationToken);
        return response is null
            ? Result<Response>.Failure(UserErrors.NotFound)
            : Result<Response>.Success(response);
    }
}
