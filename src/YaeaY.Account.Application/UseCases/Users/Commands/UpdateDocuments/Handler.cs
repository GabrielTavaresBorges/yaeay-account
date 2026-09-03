using MediatR;
using Microsoft.Extensions.Logging;
using YaeaY.Account.Application.Services.DocumentImages.Interfaces;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Entities.UserDocuments;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Documents;

namespace YaeaY.Account.Application.UseCases.Users.Commands.UpdateDocuments;

public sealed class Handler(IUserRepository userRepository, IUnitOfWork unitOfWork, IDocumentImageStorage documentImageStorage, ICurrentCpfDocumentWriter currentCpfDocumentWriter, ILogger<Handler> logger) : IRequestHandler<Command, Result<Response>>
{
    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdWithDocumentsAsync(command.Id, cancellationToken);
            if (user is null)
                return Result<Response>.Failure(UserErrors.NotFound);

            var cpfResponses = new List<CpfDocumentResponse>();
            var rgResponses = new List<RgDocumentResponse>();
            var toDelete = new List<string>();
            var cpfUpdates = new List<CpfUpdate>();
            var addedDocuments = new List<UserDocument>();
            var addedImages = new List<UserDocumentImage>();

            foreach (var input in command.CpfDocumentsToAdd ?? [])
            {
                var cpf = Cpf.Create(input.Number);
                if (cpf.IsFailure)
                    return Result<Response>.Failure(cpf.Error);

                var images = CreateImages(input.Images);
                var storage = await ValidateStorageAsync(user.Id, images, cancellationToken);
                if (storage.IsFailure)
                    return Result<Response>.Failure(storage.Error);
                var current = user.Documents.Where(document => document.DocumentType == DocumentType.Cpf)
                    .OrderByDescending(document => document.CreatedAt)
                    .FirstOrDefault();

                var unchanged = current?.Cpf?.Cpf.Number == cpf.Value.Number && SameImages(current.Images, images);
                if (unchanged)
                    continue;
                toDelete.AddRange(user.Documents.Where(document => document.DocumentType == DocumentType.Cpf)
                    .SelectMany(document => document.Images)
                    .Select(image => image.StorageObjectKey)
                    .Except(images.Select(image => image.StorageObjectKey), StringComparer.Ordinal));
                cpfUpdates.Add(new CpfUpdate(cpf.Value.Number, images));
            }

            foreach (var input in command.RgDocumentsToAdd ?? [])
            {
                var rg = Rg.Create(input.Number, input.IssuedAt, input.IssuingAuthority, input.IssuingState);
                if (rg.IsFailure)
                    return Result<Response>.Failure(rg.Error);

                var images = CreateImages(input.Images);
                var storage = await ValidateStorageAsync(user.Id, images, cancellationToken);
                if (storage.IsFailure)
                    return Result<Response>.Failure(storage.Error);

                var current = user.Documents.Where(document => document.DocumentType == DocumentType.Rg)
                    .OrderByDescending(document => document.CreatedAt)
                    .FirstOrDefault();

                var oldKeys = (current?.Images ?? []).Select(image => image.StorageObjectKey).ToArray();
                var document = user.UpsertRgDocument(rg.Value, images, out var changed);
                if (!changed)
                    continue;

                if (current is null)
                    addedDocuments.Add(document);
                else
                    addedImages.AddRange(images);

                toDelete.AddRange(oldKeys.Except(images.Select(image => image.StorageObjectKey), StringComparer.Ordinal));
                rgResponses.Add(ToResponse(document));
            }

            if (cpfUpdates.Count == 0 && rgResponses.Count == 0)
                return Result<Response>.Success(new Response(user.Id, [], [], false, "No changes to apply."));

            await unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
            {
                foreach (var update in cpfUpdates)
                {
                    var written = await currentCpfDocumentWriter.ReplaceAsync(
                        user.Id, update.Number,
                        update.Images.Select(image => new CpfDocumentImageWriteModel(
                            image.Position,
                            image.StorageObjectKey,
                            image.OriginalFileName,
                            image.ContentType,
                            image.FileSizeBytes,
                            image.Sha256Hash)).ToArray(),
                        transactionCancellationToken);

                    cpfResponses.Add(new CpfDocumentResponse(
                        written.DocumentId,
                        written.CpfId,
                        written.Number,
                        "BR",
                        written.CreatedAt,
                        written.Images.Select(image => new DocumentImageResponse(
                            image.Id,
                            image.Position,
                            image.StorageObjectKey,
                            image.OriginalFileName,
                            image.ContentType,
                            image.FileSizeBytes,
                            image.Sha256Hash,
                            image.CreatedAt)).ToArray()));
                    user.RegisterDocumentChanged();
                }

                await userRepository.UpdateUserDocumentsAsync(user, addedDocuments, addedImages, transactionCancellationToken);
                await unitOfWork.CommitAsync(transactionCancellationToken);

                return 0;

            }, cancellationToken);

            foreach (var key in toDelete.Distinct(StringComparer.Ordinal))
            {
                try
                {
                    await documentImageStorage.DeleteAsync(key, cancellationToken);
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception, "Unable to remove replaced document image {StorageObjectKey} for user {UserId}.", key, user.Id);
                }
            }

            return Result<Response>.Success(new Response(user.Id, cpfResponses, rgResponses, true, "Documents updated successfully."));
        }
        catch (DomainException exception)
        {
            logger.LogWarning(exception, "Domain error updating documents for user {UserId}.", command.Id);
            return Result<Response>.Failure(exception.Error);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error updating documents for user {UserId}.", command.Id);
            return Result<Response>.Failure(new Error(
                "unexpected.error",
                "An unexpected error occurred.",
                ErrorCategory.Unexpected,
                ErrorRule.Unexpected));
        }
    }
    private async Task<Result<bool>> ValidateStorageAsync(Guid userId, IReadOnlyCollection<UserDocumentImage> images, CancellationToken cancellationToken)
    {
        if (images.Any(image => !image.StorageObjectKey.StartsWith($"users/{userId:N}/", StringComparison.Ordinal))
            || !(await Task.WhenAll(images
            .Select(image => documentImageStorage
            .ExistsAsync(image.StorageObjectKey, cancellationToken)))
            .ConfigureAwait(false)).All(exists => exists))
            return Result<bool>.Failure(new Error(
                "document_image.not_found",
                "Uma ou mais imagens do documento não estão disponíveis para salvar.",
                ErrorCategory.Validation,
                ErrorRule.NotFound));

        return Result<bool>.Success(true);
    }

    private static UserDocumentImage[] CreateImages(IReadOnlyCollection<DocumentImageInput>? inputs) => (inputs ?? [])
        .Select(image => UserDocumentImage.Create(
            image.Position,
            image.StorageObjectKey, 
            image.OriginalFileName,
            image.ContentType,
            image.FileSizeBytes,
            image.Sha256Hash))
        .ToArray();

    private static bool SameImages(IReadOnlyCollection<UserDocumentImage> left, IReadOnlyCollection<UserDocumentImage> right) => 
        left.Count == right.Count && left
        .OrderBy(image => image.Position)
        .Select(image => image.StorageObjectKey)
        .SequenceEqual(right.OrderBy(image => image.Position).Select(image => image.StorageObjectKey), StringComparer.Ordinal);
    private static RgDocumentResponse ToResponse(UserDocument document)
    {
        var rg = document.Rg ?? throw new InvalidOperationException("An RG document must contain its RG detail."); 
        return new RgDocumentResponse(
            document.Id,
            rg.Id,
            rg.Rg.Number,
            rg.Rg.IssuedAt,
            rg.Rg.IssuingAuthority,
            rg.Rg.IssuingState,
            document.CreatedAt,
            document.Images.Select(image => new DocumentImageResponse(
                image.Id,
                image.Position,
                image.StorageObjectKey,
                image.OriginalFileName,
                image.ContentType,
                image.FileSizeBytes,
                image.Sha256Hash,
                image.CreatedAt)).ToArray()); }
    private sealed record CpfUpdate(string Number, IReadOnlyCollection<UserDocumentImage> Images);
}
