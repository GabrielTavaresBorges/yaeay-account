namespace YaeaY.Account.Application.UseCases.Users.Queries.GetDocumentImage;

public sealed record Response(Stream Content, string ContentType, string OriginalFileName);
