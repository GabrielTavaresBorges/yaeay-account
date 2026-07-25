using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Application.Services.Security.Interfaces;

public interface IPasswordHasher
{
    Result<PasswordHash> Hash(PasswordText password);
    bool Verify(PasswordHash passwordHash, string providedPassword);
}
