using FluentValidation;
using YaeaY.Account.Application.Validation;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.ResetPassword;

public sealed class CommandValidator : AbstractValidator<Command>
{
    public CommandValidator()
    {
        RuleFor(command => command.NewPassword).Custom((value, context) =>
        {
            var result = PasswordText.Create(value);
            if (result.IsFailure) context.AddDomainFailure(nameof(Command.NewPassword), result.Error);
        });
        RuleFor(command => command.ConfirmPassword).Equal(command => command.NewPassword).WithState(_ => new Error(
            "password-recovery.password-confirmation.does-not-match", "Password confirmation does not match.", ErrorCategory.Validation, ErrorRule.InvalidValue));
    }
}
