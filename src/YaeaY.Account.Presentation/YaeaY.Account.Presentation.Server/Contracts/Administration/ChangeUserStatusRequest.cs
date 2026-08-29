using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Presentation.Server.Contracts.Administration;

public sealed record ChangeUserStatusRequest(AccountStatus Status, SuspensionReason? SuspensionReason, DateTimeOffset? SuspendedUntilUtc, string Justification);
