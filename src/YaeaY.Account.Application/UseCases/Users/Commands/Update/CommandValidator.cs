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

        RuleFor(command => command.CpfDocumentsToAdd).Custom(ValidateCpfDocuments);
    }

    private static void ValidateCpfDocuments(IReadOnlyCollection<CpfDocumentInput>? documents, ValidationContext<Command> context)
    {
        if (documents is null) return;

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
