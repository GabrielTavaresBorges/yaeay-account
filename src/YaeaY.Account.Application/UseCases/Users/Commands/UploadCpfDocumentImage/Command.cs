using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.Users.Commands.UploadCpfDocumentImage;

public sealed record Command(
    Guid UserId,
    Stream Content,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes) : IRequest<Result<Response>>;
