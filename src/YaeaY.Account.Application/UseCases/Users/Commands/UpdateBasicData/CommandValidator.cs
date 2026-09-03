using FluentValidation;
using YaeaY.Account.Application.Validation;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Names;

namespace YaeaY.Account.Application.UseCases.Users.Commands.UpdateBasicData;

public sealed class CommandValidator : AbstractValidator<Command>
{
    public CommandValidator()
    {
        RuleFor(command => command.Id).Custom((id, context) =>
        {
            if (id == Guid.Empty) context.AddDomainFailure(nameof(Command.Id), UserErrors.IdRequired);
        });
        RuleFor(command => command.FullName).Custom((value, context) =>
        {
            if (value is not null)
            {
                var result = FullName.Create(value);
                if (result.IsFailure) context.AddDomainFailure(nameof(Command.FullName), result.Error);
            }
        });
        RuleFor(command => command.BirthDate).Custom((value, context) =>
        {
            if (value.HasValue)
            {
                var result = BirthDate.Create(value.Value);
                if (result.IsFailure) context.AddDomainFailure(nameof(Command.BirthDate), result.Error);
            }
        });
        RuleFor(command => command.Gender).Custom((value, context) =>
        {
            if (!value.HasValue) return;
            if (value.Value == Domain.Enumerators.Gender.Unknown)
                context.AddDomainFailure(nameof(Command.Gender), UserErrors.GenderRequired);
            else if (!Enum.IsDefined(value.Value))
                context.AddDomainFailure(nameof(Command.Gender), UserErrors.GenderInvalid);
        });
    }
}
