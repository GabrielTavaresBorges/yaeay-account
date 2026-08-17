using YaeaY.Account.Domain.ValueObjects.Securities;

using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.Services.Security.Interfaces;

public interface IEmailConfirmationTokenService
{
    Task<GeneratedEmailConfirmationToken> GenerateTokenAsync();

    Result<TokenHash> HashToken(string rawToken);
}
