namespace YaeaY.Account.Infrastructure.Messaging.RabbitMq;

public sealed record OutboxEventEnvelope(
    Guid EventId,
    string EventType,
    string Payload,
    DateTimeOffset OccurredOnUtc);
