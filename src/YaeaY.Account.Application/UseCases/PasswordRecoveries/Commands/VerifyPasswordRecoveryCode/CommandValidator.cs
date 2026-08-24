using FluentValidation;
using YaeaY.Account.Application.Validation;
using YaeaY.Account.Domain.Errors.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.VerifyPasswordRecoveryCode;

public sealed class CommandValidator : AbstractValidator<Command>
{
    public CommandValidator()
    {
        RuleFor(command => command.EmailAddress).Custom((value, context) =>
        {
            var result = Email.Create(value);
            if (result.IsFailure) context.AddDomainFailure(nameof(Command.EmailAddress), result.Error);
        });
        RuleFor(command => command.Code).Custom((value, context) =>
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 6 || value.Any(character => !char.IsAsciiDigit(character)))
                context.AddDomainFailure(nameof(Command.Code), PasswordRecoveryChallengeErrors.InvalidOrExpired);
        });
    }
}
