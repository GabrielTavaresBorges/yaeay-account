using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using YaeaY.Account.Infrastructure.ReadModels;

namespace YaeaY.Account.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqEventConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqEventConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled) return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(settings, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Falha no consumidor RabbitMQ do read model. Uma nova conexão será tentada em cinco segundos.");

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(RabbitMqOptions settings, CancellationToken stoppingToken)
    {

        var factory = new ConnectionFactory { HostName = settings.HostName, Port = settings.Port, VirtualHost = settings.VirtualHost, UserName = settings.UserName, Password = settings.Password };
        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await channel.ExchangeDeclareAsync(settings.ExchangeName, ExchangeType.Topic, true, false, null, false, false, stoppingToken);
        await channel.ExchangeDeclareAsync(settings.DeadLetterExchangeName, ExchangeType.Direct, true, false, null, false, false, stoppingToken);
        await channel.QueueDeclareAsync(
            settings.ReadModelDeadLetterQueueName,
            true,
            false,
            false,
            null,
            false,
            false,
            stoppingToken);
        await channel.QueueBindAsync(
            settings.ReadModelDeadLetterQueueName,
            settings.DeadLetterExchangeName,
            settings.ReadModelDeadLetterRoutingKey,
            null,
            false,
            stoppingToken);
        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = settings.DeadLetterExchangeName,
            ["x-dead-letter-routing-key"] = settings.ReadModelDeadLetterRoutingKey
        };
        await channel.QueueDeclareAsync(settings.ReadModelQueueName, true, false, false, queueArguments, false, false, stoppingToken);
        await channel.QueueBindAsync(settings.ReadModelQueueName, settings.ExchangeName, "account.user.changed.v1", null, false, stoppingToken);
        await channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                var envelope = JsonSerializer.Deserialize<OutboxEventEnvelope>(Encoding.UTF8.GetString(delivery.Body.Span));
                if (envelope is null) throw new InvalidOperationException("Envelope RabbitMQ inválido.");
                using var document = JsonDocument.Parse(envelope.Payload);
                var userId = document.RootElement.GetProperty("UserId").GetGuid();
                using var scope = scopeFactory.CreateScope();
                var projector = scope.ServiceProvider.GetRequiredService<UserMyDataProjector>();
                await projector.ProjectAsync(userId, envelope.EventId, envelope.OccurredOnUtc, stoppingToken);
                await channel.BasicAckAsync(delivery.DeliveryTag, false, stoppingToken);
                logger.LogInformation(
                    "Evento {EventId} projetado em {ProjectionName} para o usuário {UserId}.",
                    envelope.EventId,
                    UserMyDataProjector.ProjectionName,
                    userId);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Falha ao projetar a entrega {DeliveryTag}; a mensagem será direcionada para a DLQ {DeadLetterQueueName}.",
                    delivery.DeliveryTag,
                    settings.ReadModelDeadLetterQueueName);
                await channel.BasicNackAsync(delivery.DeliveryTag, false, false, stoppingToken);
            }
        };
        await channel.BasicConsumeAsync(settings.ReadModelQueueName, false, consumer, stoppingToken);

        while (connection.IsOpen && !stoppingToken.IsCancellationRequested)
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
    }
}
