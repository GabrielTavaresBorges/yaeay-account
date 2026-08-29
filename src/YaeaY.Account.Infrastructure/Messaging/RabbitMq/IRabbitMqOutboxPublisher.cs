using YaeaY.Account.Domain.Entities.AggregateRoots.OutboxMessages;

namespace YaeaY.Account.Infrastructure.Messaging.RabbitMq;

public interface IRabbitMqOutboxPublisher
{
    bool IsEnabled { get; }
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken);
}
