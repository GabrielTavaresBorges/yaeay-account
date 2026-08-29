using YaeaY.Account.Domain.Abstraction.Events;

namespace YaeaY.Account.Domain.Events.Users;

public sealed record UserEmailConfirmedDomainEvent(Guid UserId) : DomainEvent;
