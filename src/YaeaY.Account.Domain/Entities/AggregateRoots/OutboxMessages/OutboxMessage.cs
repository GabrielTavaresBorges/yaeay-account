using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Errors.OutboxMessages;
using YaeaY.Account.Domain.ValueObjects.Events;

namespace YaeaY.Account.Domain.Entities.AggregateRoots.OutboxMessages;

public sealed class OutboxMessage : Entity, IAggregateRoot
{
    private readonly SerializedDomainEvent _content = null!;
    private readonly DateTimeOffset _occurredOnUtc;
    private DateTimeOffset? _processedOnUtc;
    private DateTimeOffset? _lastAttemptOnUtc;
    private DateTimeOffset _nextAttemptOnUtc;
    private int _attemptCount;
    private string? _lastError;

    public SerializedDomainEvent Content => _content;
    public DateTimeOffset OccurredOnUtc => _occurredOnUtc;
    public DateTimeOffset? ProcessedOnUtc => _processedOnUtc;
    public DateTimeOffset? LastAttemptOnUtc => _lastAttemptOnUtc;
    public DateTimeOffset NextAttemptOnUtc => _nextAttemptOnUtc;
    public int AttemptCount => _attemptCount;
    public string? LastError => _lastError;
    public bool IsProcessed => _processedOnUtc.HasValue;

    private OutboxMessage() { }

    private OutboxMessage(
        Guid id,
        SerializedDomainEvent content,
        DateTimeOffset occurredOnUtc)
        : base(id)
    {
        _content = content;
        _occurredOnUtc = occurredOnUtc;
        _nextAttemptOnUtc = occurredOnUtc;
    }

    public static OutboxMessage Create(Guid id, SerializedDomainEvent content, DateTimeOffset occurredOnUtc)
    {
        ValidateCreation(id, content, occurredOnUtc);

        return new OutboxMessage(id, content, occurredOnUtc);
    }

    public bool CanBeProcessed(DateTimeOffset nowUtc) =>
        !IsProcessed && nowUtc >= _nextAttemptOnUtc;

    public void MarkAsProcessed(DateTimeOffset processedOnUtc)
    {
        EnsurePending();

        if (processedOnUtc == default)
            throw new DomainException(OutboxMessageErrors.ProcessedOnUtcRequired);

        if (processedOnUtc < _occurredOnUtc)
            throw new DomainException(OutboxMessageErrors.ProcessedBeforeOccurrence);

        _attemptCount++;
        _lastAttemptOnUtc = processedOnUtc;
        _processedOnUtc = processedOnUtc;
        _lastError = null;
    }

    public void RegisterFailure(string failure, DateTimeOffset attemptedOnUtc, DateTimeOffset nextAttemptOnUtc)
    {
        EnsurePending();

        if (string.IsNullOrWhiteSpace(failure))
            throw new DomainException(OutboxMessageErrors.FailureRequired);

        if (attemptedOnUtc == default)
            throw new DomainException(OutboxMessageErrors.AttemptedOnUtcRequired);

        if (attemptedOnUtc < _occurredOnUtc)
            throw new DomainException(OutboxMessageErrors.AttemptedBeforeOccurrence);

        if (nextAttemptOnUtc == default)
            throw new DomainException(OutboxMessageErrors.NextAttemptOnUtcRequired);

        if (nextAttemptOnUtc <= attemptedOnUtc)
            throw new DomainException(OutboxMessageErrors.NextAttemptNotAfterAttempt);

        _attemptCount++;
        _lastAttemptOnUtc = attemptedOnUtc;
        _nextAttemptOnUtc = nextAttemptOnUtc;
        _lastError = failure.Trim();
    }

    private static void ValidateCreation(Guid id, SerializedDomainEvent content, DateTimeOffset occurredOnUtc)
    {
        if (id == Guid.Empty)
            throw new DomainException(OutboxMessageErrors.IdRequired);

        if (content is null)
            throw new DomainException(OutboxMessageErrors.ContentRequired);

        if (occurredOnUtc == default)
            throw new DomainException(OutboxMessageErrors.OccurredOnUtcRequired);
    }

    private void EnsurePending()
    {
        if (IsProcessed)
            throw new DomainException(OutboxMessageErrors.AlreadyProcessed);
    }
}
