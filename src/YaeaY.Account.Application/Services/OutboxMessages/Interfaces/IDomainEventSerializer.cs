using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.ValueObjects.Events;

namespace YaeaY.Account.Application.Services.OutboxMessages.Interfaces;

public interface IDomainEventSerializer
{
    SerializedDomainEvent Serialize(IDomainEvent domainEvent);

    IDomainEvent Deserialize(SerializedDomainEvent serializedDomainEvent);
}
