using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using YaeaY.Account.Domain.Entities.AggregateRoots.OutboxMessages;

namespace YaeaY.Account.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqOutboxPublisher(IOptions<RabbitMqOptions> options) : IRabbitMqOutboxPublisher
{
    private readonly RabbitMqOptions _options = options.Value;
    public bool IsEnabled => _options.Enabled;

    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        if (!IsEnabled) return;

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true), cancellationToken);
        await channel.ExchangeDeclareAsync(_options.ExchangeName, ExchangeType.Topic, true, false, null, false, false, cancellationToken);

        var envelope = new OutboxEventEnvelope(message.Id, message.Content.EventType, message.Content.Payload, message.OccurredOnUtc);
        var properties = new BasicProperties { Persistent = true, ContentType = "application/json", MessageId = message.Id.ToString("D"), Type = message.Content.EventType };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
        await channel.BasicPublishAsync(_options.ExchangeName, RoutingKey(message.Content.EventType), true, properties, body, cancellationToken);
    }

    private static string RoutingKey(string eventType) => eventType.Contains(".Events.Users.", StringComparison.Ordinal)
        ? "account.user.changed.v1" : "account.domain-event.v1";
}
