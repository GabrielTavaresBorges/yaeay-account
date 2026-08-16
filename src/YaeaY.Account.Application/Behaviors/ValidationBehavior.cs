using FluentValidation;
using FluentValidation.Results;
using MediatR;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IValidationResult<TResponse>
{
    private readonly IReadOnlyCollection<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators.ToArray();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Count == 0)
            return await next(cancellationToken);

        foreach (var validator in _validators)
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResult = await validator.ValidateAsync(context, cancellationToken);

            var failure = validationResult.Errors.FirstOrDefault();
            if (failure is not null)
                return TResponse.Failure(ToDomainError(failure));
        }

        return await next(cancellationToken);
    }

    private static Error ToDomainError(ValidationFailure failure)
    {
        if (failure.CustomState is Error domainError)
            return domainError;

        return new Error(
            Code: string.IsNullOrWhiteSpace(failure.ErrorCode)
                ? "request.validation.failed"
                : failure.ErrorCode,
            Message: failure.ErrorMessage,
            Category: ErrorCategory.Validation,
            Rule: ErrorRule.InvalidValue);
    }
}
