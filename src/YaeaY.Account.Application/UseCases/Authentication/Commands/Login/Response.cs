namespace YaeaY.Account.Application.UseCases.Authentication.Commands.Login;

public sealed record Response(
    Guid UserId,
    string FullName,
    DateTimeOffset LoggedInAt);
