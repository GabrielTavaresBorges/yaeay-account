using MediatR;
using YaeaY.Account.Application.Services.DocumentImages.Interfaces;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Repositories.Users;

namespace YaeaY.Account.Application.UseCases.Users.Queries.GetDocumentImage;

public sealed class Handler(IUserRepository userRepository, IDocumentImageStorage documentImageStorage)
    : IRequestHandler<Query, Result<Response>>
{
    public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdWithDocumentsAsync(query.UserId, cancellationToken);
        if (user is null)
            return Result<Response>.Failure(UserErrors.NotFound);

        var image = user.Documents.SelectMany(document => document.Images)
            .SingleOrDefault(candidate => candidate.Id == query.ImageId);
        if (image is null)
        {
            return Result<Response>.Failure(new Error(
                "document_image.not_found",
                "Imagem do documento não encontrada.",
                ErrorCategory.NotFound,
                ErrorRule.NotFound));
        }

        var content = await documentImageStorage.OpenReadAsync(image.StorageObjectKey, cancellationToken);
        if (content is null)
        {
            return Result<Response>.Failure(new Error(
                "document_image.content_not_found",
                "O arquivo da imagem do documento não está disponível.",
                ErrorCategory.NotFound,
                ErrorRule.NotFound));
        }

        return Result<Response>.Success(new Response(content, image.ContentType, image.OriginalFileName));
    }
}
