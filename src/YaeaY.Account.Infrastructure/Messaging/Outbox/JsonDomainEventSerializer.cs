using System.Text.Json;
using YaeaY.Account.Application.Services.OutboxMessages.Interfaces;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.ValueObjects.Events;

namespace YaeaY.Account.Infrastructure.Messaging.Outbox;

public sealed class JsonDomainEventSerializer : IDomainEventSerializer
{
    public SerializedDomainEvent Serialize(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var eventType = domainEvent.GetType();
        var payload = JsonSerializer.Serialize(domainEvent, eventType);
        var contentResult = SerializedDomainEvent.Create(
            eventType.FullName ?? eventType.Name,
            payload);

        if (contentResult.IsFailure)
            throw new DomainException(contentResult.Error);

        return contentResult.Value;
    }

    public IDomainEvent Deserialize(SerializedDomainEvent serializedDomainEvent)
    {
        ArgumentNullException.ThrowIfNull(serializedDomainEvent);

        var eventType = typeof(IDomainEvent).Assembly
            .GetTypes()
            .SingleOrDefault(type =>
                type.FullName == serializedDomainEvent.EventType &&
                typeof(IDomainEvent).IsAssignableFrom(type) &&
                !type.IsAbstract &&
                !type.IsInterface);

        if (eventType is null)
        {
            throw new InvalidOperationException(
                $"Domain event type '{serializedDomainEvent.EventType}' is not registered in the Domain assembly.");
        }

        var domainEvent = JsonSerializer.Deserialize(serializedDomainEvent.Payload, eventType) as IDomainEvent;

        return domainEvent ?? throw new InvalidOperationException(
            $"Domain event '{serializedDomainEvent.EventType}' could not be deserialized.");
    }
}
