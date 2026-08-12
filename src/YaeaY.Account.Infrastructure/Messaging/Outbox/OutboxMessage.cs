using System.Text.Json;
using YaeaY.Account.Domain.Abstraction.Interfaces;

namespace YaeaY.Account.Infrastructure.Messaging.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset OccurredOnUtc { get; private set; }
    public DateTimeOffset? ProcessedOnUtc { get; private set; }
    public DateTimeOffset? LastAttemptOnUtc { get; private set; }
    public DateTimeOffset NextAttemptOnUtc { get; private set; }
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage()
    {
    }

    private OutboxMessage(
        Guid id,
        string eventType,
        string payload,
        DateTimeOffset occurredOnUtc)
    {
        Id = id;
        EventType = eventType;
        Payload = payload;
        OccurredOnUtc = occurredOnUtc;
        NextAttemptOnUtc = occurredOnUtc;
    }

    public static OutboxMessage From(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var eventType = domainEvent.GetType();

        return new OutboxMessage(
            id: domainEvent.EventId,
            eventType: eventType.FullName ?? eventType.Name,
            payload: JsonSerializer.Serialize(domainEvent, eventType),
            occurredOnUtc: domainEvent.OccurredOnUtc);
    }
}
