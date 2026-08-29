using YaeaY.Account.Domain.Abstraction.Events;

namespace YaeaY.Account.Domain.Events.Users;

public sealed record UserProfileChangedDomainEvent(Guid UserId) : DomainEvent;
