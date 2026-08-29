using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YaeaY.Account.Application.Events.Notifications;
using YaeaY.Account.Application.Services.OutboxMessages.Interfaces;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Infrastructure.Data.Context;
using YaeaY.Account.Infrastructure.Events.Publishers;
using YaeaY.Account.Infrastructure.Messaging.RabbitMq;
using YaeaY.Account.Infrastructure.Scheduling.Quartz;

namespace YaeaY.Account.Infrastructure.Messaging.Outbox;

public sealed class OutboxMessageProcessor : IOutboxMessageProcessor
{
    private readonly AppDbContext _context;
    private readonly IDomainEventSerializer _domainEventSerializer;
    private readonly MediatRDomainEventPublisher _domainEventPublisher;
    private readonly IRabbitMqOutboxPublisher _rabbitMqOutboxPublisher;
    private readonly IServiceProviderIsService _serviceProviderIsService;
    private readonly TimeProvider _timeProvider;
    private readonly OutboxProcessingScheduleOptions _options;
    private readonly ILogger<OutboxMessageProcessor> _logger;

    public OutboxMessageProcessor(
        AppDbContext context,
        IDomainEventSerializer domainEventSerializer,
        MediatRDomainEventPublisher domainEventPublisher,
        IRabbitMqOutboxPublisher rabbitMqOutboxPublisher,
        IServiceProviderIsService serviceProviderIsService,
        TimeProvider timeProvider,
        IOptions<OutboxProcessingScheduleOptions> options,
        ILogger<OutboxMessageProcessor> logger)
    {
        _context = context;
        _domainEventSerializer = domainEventSerializer;
        _domainEventPublisher = domainEventPublisher;
        _rabbitMqOutboxPublisher = rabbitMqOutboxPublisher;
        _serviceProviderIsService = serviceProviderIsService;
        _timeProvider = timeProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var nowUtc = _timeProvider.GetUtcNow();

        var messageIds = await _context.OutboxMessages
            .AsNoTracking()
            .Where(message =>
                message.ProcessedOnUtc == null &&
                message.NextAttemptOnUtc <= nowUtc)
            .OrderBy(message => message.NextAttemptOnUtc)
            .ThenBy(message => message.OccurredOnUtc)
            .Select(message => message.Id)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var messageId in messageIds)
            await ProcessMessageAsync(messageId, cancellationToken);
    }

    private async Task ProcessMessageAsync(Guid messageId, CancellationToken cancellationToken)
    {
        await using var transaction =  await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var message = await _context.OutboxMessages
                .SingleAsync(item => item.Id == messageId, cancellationToken);

            var domainEvent = _domainEventSerializer.Deserialize(message.Content);
            if (HasRegisteredHandler(domainEvent))
            {
                await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
            }
            else
            {
                _logger.LogDebug(
                    "O evento de domínio {DomainEventType} não possui handler local; seguirá somente para os consumidores externos.",
                    domainEvent.GetType().FullName);
            }

            await _rabbitMqOutboxPublisher.PublishAsync(message, cancellationToken);

            message.MarkAsProcessed(_timeProvider.GetUtcNow());
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();

            var failedMessage = await _context.OutboxMessages
                .SingleAsync(item => item.Id == messageId, cancellationToken);

            var attemptedOnUtc = _timeProvider.GetUtcNow();
            var nextAttemptOnUtc = attemptedOnUtc.AddSeconds(_options.RetryDelayInSeconds);

            failedMessage.RegisterFailure(exception.Message, attemptedOnUtc, nextAttemptOnUtc);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogError(
                exception,
                "Failed to process outbox message {OutboxMessageId}. Next attempt at {NextAttemptOnUtc}.",
                messageId,
                nextAttemptOnUtc);
        }
        finally
        {
            _context.ChangeTracker.Clear();
        }
    }

    private bool HasRegisteredHandler(IDomainEvent domainEvent)
    {
        var notificationType = typeof(DomainEventNotification<>)
            .MakeGenericType(domainEvent.GetType());
        var handlerType = typeof(INotificationHandler<>)
            .MakeGenericType(notificationType);

        return _serviceProviderIsService.IsService(handlerType);
    }
}
