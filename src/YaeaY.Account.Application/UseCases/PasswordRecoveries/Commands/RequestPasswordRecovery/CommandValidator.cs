using FluentValidation;
using YaeaY.Account.Application.Validation;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.RequestPasswordRecovery;

public sealed class CommandValidator : AbstractValidator<Command>
{
    public CommandValidator() => RuleFor(command => command.EmailAddress).Custom((value, context) =>
    {
        var result = Email.Create(value);
        if (result.IsFailure)
            context.AddDomainFailure(nameof(Command.EmailAddress), result.Error);
    });
}
