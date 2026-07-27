using FluentValidation;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.UseCases.Users.Commands.Create;

public sealed class CommandValidator : AbstractValidator<Command>
{
    public CommandValidator()
    {
        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .MaximumLength(254)
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(256);

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("BirthDate cannot be in the future.");

        RuleFor(x => x.Gender)
            .IsInEnum()
            .NotEqual(Gender.Unknown);

        RuleFor(x => x.PhoneType)
            .IsInEnum()
            .NotEqual(TelephoneType.Unknown);

        RuleFor(x => x.RegionCode)
            .NotEmpty()
            .Length(2)
            .Matches("^[A-Za-z]{2}$")
            .WithMessage("RegionCode must be a valid ISO2 code (e.g., BR, US).");

        RuleFor(x => x.AreaCode)
            .Matches(@"^\d+$")
            .When(x => !string.IsNullOrWhiteSpace(x.AreaCode))
            .WithMessage("AreaCode must contain digits only.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(30);
    }
}
