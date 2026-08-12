using Microsoft.EntityFrameworkCore;
using Npgsql;
using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Infrastructure.Data.Context;
using YaeaY.Account.Infrastructure.Messaging.Outbox;

namespace YaeaY.Account.Infrastructure.Data.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
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
            .Select(OutboxMessage.From)
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

    private void Detach(IEnumerable<OutboxMessage> outboxMessages)
    {
        foreach (var outboxMessage in outboxMessages)
        {
            _context.Entry(outboxMessage).State = EntityState.Detached;
        }
    }
}
