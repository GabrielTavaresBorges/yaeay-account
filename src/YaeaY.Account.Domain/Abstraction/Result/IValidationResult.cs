using YaeaY.Account.Domain.Abstraction.Errors;

namespace YaeaY.Account.Domain.Abstraction.Result;

public interface IValidationResult<TSelf>
    where TSelf : IValidationResult<TSelf>
{
    static abstract TSelf Failure(Error error);
}
