using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Repositories.Users;

namespace YaeaY.Account.Application.UseCases.Authentication.Queries.GetCurrentSession;

public sealed class Handler(IUserRepository userRepository)
    : IRequestHandler<Query, Result<Response>>
{
    public async Task<Result<Response>> Handle(
        Query query,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);

        if (user is null)
            return Result<Response>.Failure(UserErrors.NotFound);

        return Result<Response>.Success(new Response(
            UserId: user.Id,
            FullName: user.FullName.Name,
            LastLoginAt: user.LastLoginAt,
            CanManageAccount: query.CanManageAccount));
    }
}
