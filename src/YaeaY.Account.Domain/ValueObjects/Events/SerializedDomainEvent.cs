using System.Text.Json;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.SerializedDomainEvents;

namespace YaeaY.Account.Domain.ValueObjects.Events;

public sealed record SerializedDomainEvent
{
    public const int EventTypeMaximumLength = 500;

    private readonly string _eventType = string.Empty;
    private readonly string _payload = string.Empty;

    public string EventType => _eventType;
    public string Payload => _payload;

    private SerializedDomainEvent() { }

    private SerializedDomainEvent(string eventType, string payload)
    {
        _eventType = eventType;
        _payload = payload;
    }

    public static Result<SerializedDomainEvent> Create(string eventType, string payload)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            return Result<SerializedDomainEvent>.Failure(SerializedDomainEventErrors.EventTypeRequired);

        var normalizedEventType = eventType.Trim();

        if (normalizedEventType.Length > EventTypeMaximumLength)
            return Result<SerializedDomainEvent>.Failure(
                SerializedDomainEventErrors.EventTypeTooLong(normalizedEventType.Length, EventTypeMaximumLength));

        if (string.IsNullOrWhiteSpace(payload))
            return Result<SerializedDomainEvent>.Failure(SerializedDomainEventErrors.PayloadRequired);

        try
        {
            using var _ = JsonDocument.Parse(payload);
        }
        catch (JsonException)
        {
            return Result<SerializedDomainEvent>.Failure(SerializedDomainEventErrors.PayloadInvalid);
        }

        return Result<SerializedDomainEvent>.Success(new SerializedDomainEvent(normalizedEventType, payload));
    }
}
