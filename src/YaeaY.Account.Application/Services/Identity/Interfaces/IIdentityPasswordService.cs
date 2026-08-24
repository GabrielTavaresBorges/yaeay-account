using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Application.Services.Identity.Interfaces;

public interface IIdentityPasswordService
{
    Task<Result<IdentityOperation>> ResetPasswordAsync(
        Guid userId,
        PasswordText newPassword,
        CancellationToken cancellationToken = default);
}
