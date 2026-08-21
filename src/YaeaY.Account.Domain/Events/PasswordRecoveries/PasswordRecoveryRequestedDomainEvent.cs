using YaeaY.Account.Domain.Abstraction.Events;

namespace YaeaY.Account.Domain.Events.PasswordRecoveries;

public sealed record PasswordRecoveryRequestedDomainEvent(Guid ChallengeId) : DomainEvent;
