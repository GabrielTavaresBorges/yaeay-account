using YaeaY.Account.Domain.Abstraction.Events;

namespace YaeaY.Account.Domain.Events.Users;

public sealed record UserLoginRegisteredDomainEvent(Guid UserId) : DomainEvent;
