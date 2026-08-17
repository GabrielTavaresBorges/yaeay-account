using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.UseCases.EmailConfirmations.Commands.ConfirmEmail;

public sealed record Response(
    Guid UserId,
    AccountStatus Status,
    DateTimeOffset EmailConfirmedAt);
