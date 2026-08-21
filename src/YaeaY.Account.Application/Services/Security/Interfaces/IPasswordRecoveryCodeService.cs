using YaeaY.Account.Application.Services.Security.Models;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Application.Services.Security.Interfaces;

public interface IPasswordRecoveryCodeService
{
    GeneratedPasswordRecoveryCode Generate();
    bool Matches(string rawCode, PasswordRecoveryCodeHash expectedHash);
}
