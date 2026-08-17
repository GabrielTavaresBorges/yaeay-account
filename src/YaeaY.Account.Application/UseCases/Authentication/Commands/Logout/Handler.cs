using MediatR;
using YaeaY.Account.Application.Services.Identity.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.Authentication.Commands.Logout;

public sealed class Handler(IIdentityAccountService identityAccountService)
    : IRequestHandler<Command, Result<Response>>
{
    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        await identityAccountService.SignOutAsync(cancellationToken);
        return Result<Response>.Success(new Response(true));
    }
}
