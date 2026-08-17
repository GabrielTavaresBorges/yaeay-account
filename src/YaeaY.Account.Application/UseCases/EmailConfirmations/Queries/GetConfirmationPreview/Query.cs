using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.EmailConfirmations.Queries.GetConfirmationPreview;

public sealed class Query : IRequest<Result<Response>>
{
    public string Token { get; }

    public Query(string token)
    {
        Token = token;
    }

    public override string ToString() => nameof(Query);
}
