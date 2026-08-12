namespace YaeaY.Account.Domain.Abstraction.Interfaces;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredOnUtc { get; }
}
