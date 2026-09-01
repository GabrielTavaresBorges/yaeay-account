using Microsoft.EntityFrameworkCore;
using Npgsql;
using YaeaY.Account.Application.Services.OutboxMessages.Interfaces;
using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Entities.AggregateRoots.OutboxMessages;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Errors.UserDocuments;
using YaeaY.Account.Infrastructure.Data.Context;

namespace YaeaY.Account.Infrastructure.Data.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IDomainEventSerializer _domainEventSerializer;

    public UnitOfWork(
        AppDbContext context,
        IDomainEventSerializer domainEventSerializer)
    {
        _context = context;
        _domainEventSerializer = domainEventSerializer;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Any())
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        var outboxMessages = domainEvents
            .Select(CreateOutboxMessage)
            .ToList();

        _context.OutboxMessages.AddRange(outboxMessages);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "UX_User_Email"
            })
        {
            Detach(outboxMessages);
            throw new DomainException(UserErrors.EmailAlreadyInUse, ex);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "UX_UserDocumentImages_StorageObjectKey"
            })
        {
            Detach(outboxMessages);
            throw new DomainException(UserDocumentErrors.ImageStorageObjectKeyAlreadyExists, ex);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "UX_UserDocumentImages_UserDocumentId_Position"
            })
        {
            Detach(outboxMessages);
            throw new DomainException(UserDocumentErrors.ImagePositionAlreadyExists, ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Detach(outboxMessages);
            throw new DomainException(UserDocumentErrors.ChangedConcurrently, ex);
        }
        catch
        {
            Detach(outboxMessages);
            throw;
        }

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (_context.Database.CurrentTransaction is not null)
            return await operation(cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private OutboxMessage CreateOutboxMessage(IDomainEvent domainEvent)
    {
        var content = _domainEventSerializer.Serialize(domainEvent);

        return OutboxMessage.Create(
            domainEvent.EventId,
            content,
            domainEvent.OccurredOnUtc);
    }

    private void Detach(IEnumerable<OutboxMessage> outboxMessages)
    {
        foreach (var outboxMessage in outboxMessages)
        {
            _context.Entry(outboxMessage).State = EntityState.Detached;
        }
    }
}
