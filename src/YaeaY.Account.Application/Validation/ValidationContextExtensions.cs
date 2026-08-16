using FluentValidation;
using FluentValidation.Results;
using YaeaY.Account.Domain.Abstraction.Errors;

namespace YaeaY.Account.Application.Validation;

public static class ValidationContextExtensions
{
    public static void AddDomainFailure<T>(this ValidationContext<T> context, string propertyName, Error error)
    {
        context.AddFailure(new ValidationFailure(propertyName, error.Message)
        {
            ErrorCode = error.Code,
            CustomState = error
        });
    }
}
