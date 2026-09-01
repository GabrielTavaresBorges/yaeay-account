using System;
using FluentValidation;
using YaeaY.Account.Application.Validation;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.UserDocuments;
using YaeaY.Account.Domain.Errors.UserDocuments;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Documents;
using YaeaY.Account.Domain.ValueObjects.Names;
using YaeaY.Account.Domain.Errors.Telephones;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.UseCases.Users.Commands.Update;

public sealed class CommandValidator : AbstractValidator<Command>
{
    public CommandValidator()
    {
        RuleFor(command => command.Id).Custom((id, context) =>
        {
            if (id == Guid.Empty)
                context.AddDomainFailure(nameof(Command.Id), UserErrors.IdRequired);
        });

        RuleFor(command => command.FullName).Custom((fullName, context) =>
        {
            if (fullName is null) return;
            var result = FullName.Create(fullName);
            if (result.IsFailure) context.AddDomainFailure(nameof(Command.FullName), result.Error);
        });

        RuleFor(command => command.BirthDate).Custom((birthDate, context) =>
        {
            if (!birthDate.HasValue) return;
            var result = BirthDate.Create(birthDate.Value);
            if (result.IsFailure) context.AddDomainFailure(nameof(Command.BirthDate), result.Error);
        });

        RuleFor(command => command.Gender).Custom((gender, context) =>
        {
            if (!gender.HasValue) return;
            if (gender.Value == Domain.Enumerators.Gender.Unknown)
                context.AddDomainFailure(nameof(Command.Gender), UserErrors.GenderRequired);
            else if (!Enum.IsDefined(gender.Value))
                context.AddDomainFailure(nameof(Command.Gender), UserErrors.GenderInvalid);
        });

        RuleFor(command => command.Phones).Custom(ValidatePhones);

        RuleFor(command => command.CpfDocumentsToAdd).Custom(ValidateCpfDocuments);
    }

    private static void ValidatePhones(IReadOnlyCollection<PhoneInput>? phones, ValidationContext<Command> context)
    {
        if (phones is null) return;

        if (phones.Count == 0)
        {
            context.AddDomainFailure(nameof(Command.Phones), UserErrors.AtLeastOnePhoneRequired);
            return;
        }

        if (phones.Count(phone => phone.IsPrimary) != 1)
            context.AddDomainFailure(nameof(Command.Phones), UserErrors.PrimaryPhoneRequired);

        var phoneIds = new HashSet<Guid>();
        for (var index = 0; index < phones.Count; index++)
        {
            var phone = phones.ElementAt(index);
            var prefix = $"{nameof(Command.Phones)}[{index}]";

            if (phone.Id == Guid.Empty)
                context.AddDomainFailure($"{prefix}.{nameof(PhoneInput.Id)}", UserErrors.PhoneIdRequired);
            else if (phone.Id.HasValue && !phoneIds.Add(phone.Id.Value))
                context.AddDomainFailure($"{prefix}.{nameof(PhoneInput.Id)}", UserErrors.PhoneIdDuplicated);

            ValidateCallingCode(phone.CallingCode, context, $"{prefix}.{nameof(PhoneInput.CallingCode)}");
            ValidateRegionCode(phone.RegionCode, context, $"{prefix}.{nameof(PhoneInput.RegionCode)}");
            ValidateAreaCode(phone.AreaCode, context, $"{prefix}.{nameof(PhoneInput.AreaCode)}");
            ValidatePhoneType(phone.PhoneType, context, $"{prefix}.{nameof(PhoneInput.PhoneType)}");
            ValidatePhoneNumber(phone.PhoneNumber, context, $"{prefix}.{nameof(PhoneInput.PhoneNumber)}");
        }
    }

    private static void ValidateCallingCode(string callingCode, ValidationContext<Command> context, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(callingCode))
        {
            context.AddDomainFailure(propertyName, TelephoneNumberErrors.CallingCodeRequired);
            return;
        }

        var normalized = callingCode.Trim();
        if (!normalized.StartsWith('+') || normalized.Length < 2 || normalized[1..].Any(character => !char.IsDigit(character)))
            context.AddDomainFailure(propertyName, TelephoneNumberErrors.CallingCodeInvalid);
    }

    private static void ValidateRegionCode(string regionCode, ValidationContext<Command> context, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(regionCode))
        {
            context.AddDomainFailure(propertyName, TelephoneNumberErrors.RegionCodeRequired);
            return;
        }

        var normalized = regionCode.Trim().ToUpperInvariant();
        if (normalized.Length != 2 || normalized.Any(character => character is < 'A' or > 'Z'))
            context.AddDomainFailure(propertyName, TelephoneNumberErrors.RegionCodeInvalid);
    }

    private static void ValidateAreaCode(string? areaCode, ValidationContext<Command> context, string propertyName)
    {
        if (!string.IsNullOrWhiteSpace(areaCode) && areaCode.Trim().Any(character => !char.IsDigit(character)))
            context.AddDomainFailure(propertyName, TelephoneNumberErrors.AreaCodeInvalid);
    }

    private static void ValidatePhoneType(TelephoneType phoneType, ValidationContext<Command> context, string propertyName)
    {
        if (phoneType == TelephoneType.Unknown)
        {
            context.AddDomainFailure(propertyName, TelephoneNumberErrors.PhoneTypeRequired);
            return;
        }

        if (!Enum.IsDefined(phoneType))
            context.AddDomainFailure(propertyName, TelephoneNumberErrors.PhoneTypeInvalid);
    }

    private static void ValidatePhoneNumber(string phoneNumber, ValidationContext<Command> context, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            context.AddDomainFailure(propertyName, TelephoneNumberErrors.NationalNumberRequired);
            return;
        }

        if (phoneNumber.Trim().Length > 30)
            context.AddDomainFailure(propertyName, TelephoneNumberErrors.NationalNumberTooLong(phoneNumber.Trim().Length, 30));
    }

    private static void ValidateCpfDocuments(IReadOnlyCollection<CpfDocumentInput>? documents, ValidationContext<Command> context)
    {
        if (documents is null) return;

        if (documents.Count > 1)
            context.AddDomainFailure(nameof(Command.CpfDocumentsToAdd), UserDocumentErrors.CpfSingleCurrentRequired);

        var requestStorageKeys = new HashSet<string>(StringComparer.Ordinal);
        var documentIndex = 0;

        foreach (var document in documents)
        {
            var documentPath = $"{nameof(Command.CpfDocumentsToAdd)}[{documentIndex}]";
            var cpfResult = Cpf.Create(document.Number);
            if (cpfResult.IsFailure)
                context.AddDomainFailure($"{documentPath}.{nameof(CpfDocumentInput.Number)}", cpfResult.Error);

            var images = document.Images ?? [];
            if (images.Count > UserDocumentImage.MaximumPosition)
                context.AddDomainFailure($"{documentPath}.{nameof(CpfDocumentInput.Images)}", UserDocumentErrors.ImageLimitExceeded);

            var positions = new HashSet<short>();
            var imageIndex = 0;
            foreach (var image in images)
            {
                var imagePath = $"{documentPath}.{nameof(CpfDocumentInput.Images)}[{imageIndex}]";
                try
                {
                    _ = UserDocumentImage.Create(image.Position, image.StorageObjectKey!, image.OriginalFileName, image.ContentType, image.FileSizeBytes, image.Sha256Hash);
                }
                catch (DomainException exception)
                {
                    context.AddDomainFailure(imagePath, exception.Error);
                }

                if (!positions.Add(image.Position))
                    context.AddDomainFailure($"{imagePath}.{nameof(DocumentImageInput.Position)}", UserDocumentErrors.ImagePositionAlreadyExists);

                var storageKey = image.StorageObjectKey?.Trim() ?? string.Empty;
                if (storageKey.Length > 0 && !requestStorageKeys.Add(storageKey))
                    context.AddDomainFailure($"{imagePath}.{nameof(DocumentImageInput.StorageObjectKey)}", UserDocumentErrors.ImageStorageObjectKeyAlreadyExists);

                imageIndex++;
            }

            documentIndex++;
        }
    }
}
