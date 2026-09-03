using FluentValidation;
using YaeaY.Account.Application.Validation;
using YaeaY.Account.Domain.Errors.Telephones;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.UseCases.Users.Commands.UpdatePhones;

public sealed class CommandValidator : AbstractValidator<Command>
{
    public CommandValidator()
    {
        RuleFor(command => command.Id).Custom((id, context) => { if (id == Guid.Empty) context.AddDomainFailure(nameof(Command.Id), UserErrors.IdRequired); });
        RuleFor(command => command.Phones).Custom(ValidatePhones);
    }

    private static void ValidatePhones(IReadOnlyCollection<PhoneInput>? phones, ValidationContext<Command> context)
    {
        if (phones is null) return;
        if (phones.Count == 0) { context.AddDomainFailure(nameof(Command.Phones), UserErrors.AtLeastOnePhoneRequired); return; }
        if (phones.Count(phone => phone.IsPrimary) != 1) context.AddDomainFailure(nameof(Command.Phones), UserErrors.PrimaryPhoneRequired);
        var ids = new HashSet<Guid>();
        for (var index = 0; index < phones.Count; index++)
        {
            var phone = phones.ElementAt(index);
            var path = $"{nameof(Command.Phones)}[{index}]";
            if (phone.Id == Guid.Empty) context.AddDomainFailure($"{path}.{nameof(PhoneInput.Id)}", UserErrors.PhoneIdRequired);
            else if (phone.Id.HasValue && !ids.Add(phone.Id.Value)) context.AddDomainFailure($"{path}.{nameof(PhoneInput.Id)}", UserErrors.PhoneIdDuplicated);
            if (string.IsNullOrWhiteSpace(phone.CallingCode)) context.AddDomainFailure($"{path}.{nameof(PhoneInput.CallingCode)}", TelephoneNumberErrors.CallingCodeRequired);
            else if (!phone.CallingCode.Trim().StartsWith('+') || phone.CallingCode.Trim().Length < 2 || phone.CallingCode.Trim()[1..].Any(character => !char.IsDigit(character))) context.AddDomainFailure($"{path}.{nameof(PhoneInput.CallingCode)}", TelephoneNumberErrors.CallingCodeInvalid);
            if (string.IsNullOrWhiteSpace(phone.RegionCode)) context.AddDomainFailure($"{path}.{nameof(PhoneInput.RegionCode)}", TelephoneNumberErrors.RegionCodeRequired);
            else { var region = phone.RegionCode.Trim(); if (region.Length != 2 || region.Any(character => character is < 'A' or > 'Z')) context.AddDomainFailure($"{path}.{nameof(PhoneInput.RegionCode)}", TelephoneNumberErrors.RegionCodeInvalid); }
            if (!string.IsNullOrWhiteSpace(phone.AreaCode) && phone.AreaCode.Trim().Any(character => !char.IsDigit(character))) context.AddDomainFailure($"{path}.{nameof(PhoneInput.AreaCode)}", TelephoneNumberErrors.AreaCodeInvalid);
            if (phone.PhoneType == TelephoneType.Unknown) context.AddDomainFailure($"{path}.{nameof(PhoneInput.PhoneType)}", TelephoneNumberErrors.PhoneTypeRequired);
            else if (!Enum.IsDefined(phone.PhoneType)) context.AddDomainFailure($"{path}.{nameof(PhoneInput.PhoneType)}", TelephoneNumberErrors.PhoneTypeInvalid);
            if (string.IsNullOrWhiteSpace(phone.PhoneNumber)) context.AddDomainFailure($"{path}.{nameof(PhoneInput.PhoneNumber)}", TelephoneNumberErrors.NationalNumberRequired);
            else if (phone.PhoneNumber.Trim().Length > 30) context.AddDomainFailure($"{path}.{nameof(PhoneInput.PhoneNumber)}", TelephoneNumberErrors.NationalNumberTooLong(phone.PhoneNumber.Trim().Length, 30));
        }
    }
}
