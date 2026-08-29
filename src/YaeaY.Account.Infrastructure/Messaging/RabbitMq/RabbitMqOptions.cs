namespace YaeaY.Account.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqOptions
{
    public const string SectionName = "Messaging:RabbitMq";
    public bool Enabled { get; init; }
    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string VirtualHost { get; init; } = "/";
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = string.Empty;
    public string ExchangeName { get; init; } = "account.events";
    public string ReadModelQueueName { get; init; } = "account.read-model";
    public string DeadLetterExchangeName { get; init; } = "account.dead-letter";
    public string ReadModelDeadLetterQueueName { get; init; } = "account.read-model.dead-letter";
    public string ReadModelDeadLetterRoutingKey { get; init; } = "account.read-model.failed.v1";
}
