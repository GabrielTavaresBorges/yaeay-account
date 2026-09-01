using MediatR;
using Microsoft.Extensions.Logging;
using YaeaY.Account.Application.Services.DocumentImages.Interfaces;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.Users.Commands.UploadCpfDocumentImage;

public sealed class Handler(IDocumentImageStorage documentImageStorage, ILogger<Handler> logger)
    : IRequestHandler<Command, Result<Response>>
{
    private const long MaximumFileSizeBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        if (command.UserId == Guid.Empty || command.Content is null || command.FileSizeBytes is <= 0 or > MaximumFileSizeBytes
            || string.IsNullOrWhiteSpace(command.OriginalFileName) || !AllowedContentTypes.Contains(command.ContentType))
        {
            return Result<Response>.Failure(new Error(
                "document_image.invalid",
                "Envie uma imagem JPEG, PNG ou WebP de até 5 MB.",
                ErrorCategory.Validation,
                ErrorRule.InvalidValue));
        }

        try
        {
            var stored = await documentImageStorage.StoreCpfImageAsync(
                command.UserId, command.Content, command.OriginalFileName, command.ContentType,
                command.FileSizeBytes, cancellationToken);

            return Result<Response>.Success(new Response(
                stored.StorageObjectKey, stored.OriginalFileName, stored.ContentType,
                stored.FileSizeBytes, stored.Sha256Hash));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to store CPF image for user {UserId}.", command.UserId);
            return Result<Response>.Failure(new Error(
                "document_image.storage_failed",
                "Não foi possível armazenar a imagem do documento.",
                ErrorCategory.Unexpected,
                ErrorRule.Unexpected));
        }
    }
}
