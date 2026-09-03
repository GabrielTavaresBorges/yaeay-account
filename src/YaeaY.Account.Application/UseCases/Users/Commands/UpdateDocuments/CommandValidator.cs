using FluentValidation;
using YaeaY.Account.Application.Validation;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.UserDocuments;
using YaeaY.Account.Domain.Errors.UserDocuments;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.ValueObjects.Documents;

namespace YaeaY.Account.Application.UseCases.Users.Commands.UpdateDocuments;

public sealed class CommandValidator : AbstractValidator<Command>
{
    public CommandValidator()
    {
        RuleFor(command => command.Id).Custom((id, context) => { if (id == Guid.Empty) context.AddDomainFailure(nameof(Command.Id), UserErrors.IdRequired); });
        RuleFor(command => command.CpfDocumentsToAdd).Custom(ValidateCpfDocuments);
        RuleFor(command => command.RgDocumentsToAdd).Custom(ValidateRgDocuments);
    }
    private static void ValidateCpfDocuments(IReadOnlyCollection<CpfDocumentInput>? documents, ValidationContext<Command> context)
    {
        if (documents is null) return;
        if (documents.Count > 1) context.AddDomainFailure(nameof(Command.CpfDocumentsToAdd), UserDocumentErrors.CpfSingleCurrentRequired);
        ValidateDocuments(documents, nameof(Command.CpfDocumentsToAdd), context, input => Cpf.Create(input.Number).ErrorOrNull());
    }
    private static void ValidateRgDocuments(IReadOnlyCollection<RgDocumentInput>? documents, ValidationContext<Command> context)
    {
        if (documents is null) return;
        if (documents.Count > 1) context.AddDomainFailure(nameof(Command.RgDocumentsToAdd), UserDocumentErrors.RgSingleCurrentRequired);
        var index = 0;
        foreach (var input in documents)
        {
            var result = Rg.Create(input.Number, input.IssuedAt, input.IssuingAuthority, input.IssuingState);
            if (result.IsFailure) context.AddDomainFailure($"{nameof(Command.RgDocumentsToAdd)}[{index}]", result.Error);
            ValidateImages(input.Images, $"{nameof(Command.RgDocumentsToAdd)}[{index}]", context);
            index++;
        }
    }
    private static void ValidateDocuments(IReadOnlyCollection<CpfDocumentInput> documents, string root, ValidationContext<Command> context, Func<CpfDocumentInput, YaeaY.Account.Domain.Abstraction.Errors.Error?> validation)
    {
        var index = 0;
        foreach (var input in documents)
        {
            var error = validation(input);
            if (error is not null) context.AddDomainFailure($"{root}[{index}].{nameof(CpfDocumentInput.Number)}", error);
            ValidateImages(input.Images, $"{root}[{index}]", context);
            index++;
        }
    }
    private static void ValidateImages(IReadOnlyCollection<DocumentImageInput>? images, string documentPath, ValidationContext<Command> context)
    {
        var values = images ?? [];
        if (values.Count > UserDocumentImage.MaximumPosition) context.AddDomainFailure($"{documentPath}.Images", UserDocumentErrors.ImageLimitExceeded);
        var positions = new HashSet<short>();
        var keys = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < values.Count; index++)
        {
            var image = values.ElementAt(index);
            var path = $"{documentPath}.Images[{index}]";
            try { _ = UserDocumentImage.Create(image.Position, image.StorageObjectKey, image.OriginalFileName, image.ContentType, image.FileSizeBytes, image.Sha256Hash); }
            catch (DomainException exception) { context.AddDomainFailure(path, exception.Error); }
            if (!positions.Add(image.Position)) context.AddDomainFailure($"{path}.{nameof(DocumentImageInput.Position)}", UserDocumentErrors.ImagePositionAlreadyExists);
            var key = image.StorageObjectKey?.Trim() ?? string.Empty;
            if (key.Length > 0 && !keys.Add(key)) context.AddDomainFailure($"{path}.{nameof(DocumentImageInput.StorageObjectKey)}", UserDocumentErrors.ImageStorageObjectKeyAlreadyExists);
        }
    }
}

internal static class ResultExtensions
{
    public static YaeaY.Account.Domain.Abstraction.Errors.Error? ErrorOrNull<T>(this YaeaY.Account.Domain.Abstraction.Result.Result<T> result) => result.IsFailure ? result.Error : null;
}
