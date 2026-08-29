using YaeaY.Account.Domain.Abstraction.Events;

namespace YaeaY.Account.Domain.Events.Users;

public sealed record UserDocumentAddedDomainEvent(Guid UserId) : DomainEvent;
