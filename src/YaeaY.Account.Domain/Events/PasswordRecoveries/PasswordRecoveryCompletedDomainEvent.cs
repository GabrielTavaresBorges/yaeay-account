using YaeaY.Account.Domain.Abstraction.Events;

namespace YaeaY.Account.Domain.Events.PasswordRecoveries;

public sealed record PasswordRecoveryCompletedDomainEvent(Guid ChallengeId) : DomainEvent;
