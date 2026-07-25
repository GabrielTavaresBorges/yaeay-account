using YaeaY.Account.Domain.Abstraction.Events;

namespace YaeaY.Account.Domain.Events.Users;

public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Email,
    string FullName) : DomainEvent;
