using FluentValidation;
using YaeaY.Account.Application.Validation;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Telephones;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Application.UseCases.Users.Commands.Create;

public sealed class CommandValidator : AbstractValidator<Command>
{
    public CommandValidator()
    {
        RuleFor(command => command.EmailAddress)
            .Custom((value, context) =>
                AddFailureWhenInvalid(Email.Create(value), context, nameof(Command.EmailAddress)));

        RuleFor(command => command.Password)
            .Custom((value, context) =>
                AddFailureWhenInvalid(PasswordText.Create(value), context, nameof(Command.Password)));

        RuleFor(command => command.FullName)
            .Custom((value, context) =>
                AddFailureWhenInvalid(FullName.Create(value), context, nameof(Command.FullName)));

        RuleFor(command => command.BirthDate)
            .Custom((value, context) =>
                AddFailureWhenInvalid(BirthDate.Create(value), context, nameof(Command.BirthDate)));

        RuleFor(command => command.Gender)
            .Custom(ValidateGender);

        RuleFor(command => command.CallingCode)
            .Custom(ValidateCallingCode);

        RuleFor(command => command.PhoneType)
            .Custom(ValidatePhoneType);

        RuleFor(command => command.RegionCode)
            .Custom(ValidateRegionCode);

        RuleFor(command => command.AreaCode)
            .Custom(ValidateAreaCode);

        RuleFor(command => command.PhoneNumber)
            .Custom(ValidatePhoneNumber);
    }

    private static void ValidateGender(Gender gender, ValidationContext<Command> context)
    {
        if (gender == Gender.Unknown)
        {
            context.AddDomainFailure(nameof(Command.Gender), UserErrors.GenderRequired);
            return;
        }

        if (!Enum.IsDefined(gender))
            context.AddDomainFailure(nameof(Command.Gender), UserErrors.GenderInvalid);
    }

    private static void ValidateCallingCode(string callingCode, ValidationContext<Command> context)
    {
        if (string.IsNullOrWhiteSpace(callingCode))
        {
            context.AddDomainFailure(nameof(Command.CallingCode), TelephoneNumberErrors.CallingCodeRequired);
            return;
        }

        var normalized = callingCode.Trim();
        if (!normalized.StartsWith('+') || normalized.Length < 2 || normalized[1..].Any(character => !char.IsDigit(character)))
            context.AddDomainFailure(nameof(Command.CallingCode), TelephoneNumberErrors.CallingCodeInvalid);

    }

    private static void ValidateRegionCode(string regionCode, ValidationContext<Command> context)
    {
        if (string.IsNullOrWhiteSpace(regionCode))
        {
            context.AddDomainFailure(nameof(Command.RegionCode), TelephoneNumberErrors.RegionCodeRequired);
            return;
        }

        var normalized = regionCode.Trim().ToUpperInvariant();
        if (normalized.Length != 2 || normalized.Any(character => character is < 'A' or > 'Z'))
            context.AddDomainFailure(nameof(Command.RegionCode), TelephoneNumberErrors.RegionCodeInvalid);
    }

    private static void ValidateAreaCode(string? areaCode, ValidationContext<Command> context)
    {
        if (!string.IsNullOrWhiteSpace(areaCode) && areaCode.Trim().Any(character => !char.IsDigit(character)))
            context.AddDomainFailure(nameof(Command.AreaCode), TelephoneNumberErrors.AreaCodeInvalid);
    }

    private static void ValidatePhoneType(TelephoneType phoneType, ValidationContext<Command> context)
    {
        if (phoneType == TelephoneType.Unknown)
        {
            context.AddDomainFailure(nameof(Command.PhoneType), TelephoneNumberErrors.PhoneTypeRequired);
            return;
        }

        if (!Enum.IsDefined(phoneType))
            context.AddDomainFailure(nameof(Command.PhoneType), TelephoneNumberErrors.PhoneTypeInvalid);
    }

    private static void ValidatePhoneNumber(string phoneNumber, ValidationContext<Command> context)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            context.AddDomainFailure(nameof(Command.PhoneNumber), TelephoneNumberErrors.NationalNumberRequired);
            return;
        }

        const int maximumLength = 30;
        var normalized = phoneNumber.Trim();

        if (normalized.Length > maximumLength)
            context.AddDomainFailure(nameof(Command.PhoneNumber), TelephoneNumberErrors.NationalNumberTooLong(
                    normalized.Length,
                    maximumLength));
    }

    private static void AddFailureWhenInvalid<TValue>(Result<TValue> result, ValidationContext<Command> context, string propertyName)
    {
        if (result.IsFailure)
            context.AddDomainFailure(propertyName, result.Error);
    }
}
