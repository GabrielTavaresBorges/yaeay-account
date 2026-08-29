namespace YaeaY.Account.Application.UseCases.Authentication.Queries.GetCurrentSession;

public sealed record Response(
    Guid UserId,
    string FullName,
    DateTimeOffset? LastLoginAt,
    bool CanManageAccount);
