using YaeaY.Account.Domain.Abstraction.Interfaces;

namespace YaeaY.Account.Domain.Abstraction.Events;

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;
}
