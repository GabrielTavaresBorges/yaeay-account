using FluentValidation;
using YaeaY.Account.Application.Validation;
using YaeaY.Account.Domain.Errors.PasswordText;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Application.UseCases.Authentication.Commands.Login;

public sealed class CommandValidator : AbstractValidator<Command>
{
    public CommandValidator()
    {
        RuleFor(command => command.EmailAddress)
            .Custom((value, context) =>
            {
                var result = Email.Create(value);
                if (result.IsFailure)
                    context.AddDomainFailure(nameof(Command.EmailAddress), result.Error);
            });

        RuleFor(command => command.Password)
            .Custom((value, context) =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    context.AddDomainFailure(nameof(Command.Password), PasswordTextErrors.Required);
                    return;
                }

                const int maximumLength = 256;
                if (value.Length > maximumLength)
                {
                    context.AddDomainFailure(
                        nameof(Command.Password),
                        PasswordTextErrors.TooLong(value.Length, maximumLength));
                }
            });
    }
}
