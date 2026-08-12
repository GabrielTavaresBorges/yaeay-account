using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Events;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Infrastructure.Data.Context;
using YaeaY.Account.Infrastructure.Data.Persistence;
using YaeaY.Account.Infrastructure.Messaging.Outbox;

namespace YaeaY.Account.Infrastructure.UnitTests.Data.PersistenceTests;

public sealed class UnitOfWorkTests
{
    [Fact]
    public async Task CommitAsync_WhenThereAreNoDomainEvents_ShouldSaveChangesWithoutCreatingOutboxMessages()
    {
        // Arrange

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        using var context = CreateContext();
        var unitOfWork = CreateUnitOfWork(context);

        // Act

        await unitOfWork.CommitAsync(cancellationToken);

        // Assert

        context.SaveChangesCallCount.Should().Be(1);
        context.SaveChangesCancellationToken.Should().Be(cancellationToken);
        context.OutboxMessages.Local.Should().BeEmpty();
    }

    [Fact]
    public async Task CommitAsync_WhenEntityHasDomainEvent_ShouldSaveOutboxMessageAndClearDomainEvents()
    {
        // Arrange

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var domainEvent = new TestDomainEvent("test-value");
        var entity = new TestEntity(domainEvent);

        using var context = CreateContext();
        context.Add(entity);

        var unitOfWork = CreateUnitOfWork(context);

        // Act

        await unitOfWork.CommitAsync(cancellationToken);

        // Assert

        context.SaveChangesCallCount.Should().Be(1);
        context.SaveChangesCancellationToken.Should().Be(cancellationToken);
        entity.DomainEvents.Should().BeEmpty();

        var outboxMessage = context.OutboxMessages.Local
            .Should()
            .ContainSingle()
            .Which;

        outboxMessage.Id.Should().Be(domainEvent.EventId);
        outboxMessage.Content.EventType.Should().Be(typeof(TestDomainEvent).FullName);
        outboxMessage.Content.Payload.Should().Contain("test-value");
        outboxMessage.OccurredOnUtc.Should().Be(domainEvent.OccurredOnUtc);
        outboxMessage.NextAttemptOnUtc.Should().Be(domainEvent.OccurredOnUtc);
        outboxMessage.ProcessedOnUtc.Should().BeNull();
        outboxMessage.AttemptCount.Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_WhenUserEmailUniqueConstraintIsViolated_ShouldThrowDomainException_WithUserErrorsEmailAlreadyInUse()
    {
        // Arrange

        var postgresException = CreatePostgresException(
            PostgresErrorCodes.UniqueViolation,
            "UX_User_Email");

        var dbUpdateException = new DbUpdateException(
            "An error occurred while saving the entity changes.",
            postgresException);

        using var context = CreateContext();
        context.SaveChangesException = dbUpdateException;

        var unitOfWork = CreateUnitOfWork(context);

        // Act

        Func<Task> act = () => unitOfWork.CommitAsync();

        // Assert

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.Error.Should().Be(UserErrors.EmailAlreadyInUse);
        exception.Which.InnerException.Should().BeSameAs(dbUpdateException);
        context.SaveChangesCallCount.Should().Be(1);
        context.OutboxMessages.Local.Should().BeEmpty();
    }

    [Theory]
    [InlineData(PostgresErrorCodes.UniqueViolation, "UX_Other")]
    [InlineData(PostgresErrorCodes.ForeignKeyViolation, "UX_User_Email")]
    public async Task CommitAsync_WhenPostgresErrorDoesNotMatchUserEmailUniqueConstraint_ShouldRethrowDbUpdateException(
        string sqlState,
        string constraintName)
    {
        // Arrange

        var postgresException = CreatePostgresException(sqlState, constraintName);
        var dbUpdateException = new DbUpdateException(
            "An error occurred while saving the entity changes.",
            postgresException);

        using var context = CreateContext();
        context.SaveChangesException = dbUpdateException;

        var unitOfWork = CreateUnitOfWork(context);

        // Act

        Func<Task> act = () => unitOfWork.CommitAsync();

        // Assert

        var exception = await act.Should().ThrowAsync<DbUpdateException>();
        exception.Which.Should().BeSameAs(dbUpdateException);
        context.SaveChangesCallCount.Should().Be(1);
        context.OutboxMessages.Local.Should().BeEmpty();
    }

    [Fact]
    public async Task CommitAsync_WhenSaveChangesFails_ShouldKeepDomainEventAndDetachOutboxMessage()
    {
        // Arrange

        var domainEvent = new TestDomainEvent("test-value");
        var entity = new TestEntity(domainEvent);
        var dbUpdateException = new DbUpdateException("Save failed.");

        using var context = CreateContext();
        context.Add(entity);
        context.SaveChangesException = dbUpdateException;

        var unitOfWork = CreateUnitOfWork(context);

        // Act

        Func<Task> act = () => unitOfWork.CommitAsync();

        // Assert

        var exception = await act.Should().ThrowAsync<DbUpdateException>();
        exception.Which.Should().BeSameAs(dbUpdateException);
        entity.DomainEvents.Should().ContainSingle().Which.Should().BeSameAs(domainEvent);
        context.OutboxMessages.Local.Should().BeEmpty();
    }

    private static TestAppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
            .Options;

        return new TestAppDbContext(options);
    }

    private static UnitOfWork CreateUnitOfWork(AppDbContext context)
    {
        return new UnitOfWork(context, new JsonDomainEventSerializer());
    }

    private static PostgresException CreatePostgresException(
        string sqlState,
        string constraintName)
    {
        return new PostgresException(
            messageText: "Database constraint violation.",
            severity: "ERROR",
            invariantSeverity: "ERROR",
            sqlState: sqlState,
            detail: null,
            hint: null,
            position: 0,
            internalPosition: 0,
            internalQuery: null,
            where: null,
            schemaName: "public",
            tableName: "Users",
            columnName: "Email",
            dataTypeName: null,
            constraintName: constraintName,
            file: "nbtinsert.c",
            line: "666",
            routine: "_bt_check_unique");
    }

    private sealed class TestAppDbContext(DbContextOptions<AppDbContext> options)
        : AppDbContext(options)
    {
        public Exception? SaveChangesException { get; set; }
        public int SaveChangesCallCount { get; private set; }
        public CancellationToken SaveChangesCancellationToken { get; private set; }

        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            SaveChangesCancellationToken = cancellationToken;

            if (SaveChangesException is not null)
            {
                return Task.FromException<int>(SaveChangesException);
            }

            return Task.FromResult(1);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestEntity>(builder =>
            {
                builder.HasKey(entity => entity.Id);
                builder.Ignore(entity => entity.DomainEvents);
            });
        }
    }

    private sealed class TestEntity : Entity
    {
        private TestEntity()
        {
        }

        public TestEntity(TestDomainEvent domainEvent)
        {
            AddDomainEvent(domainEvent);
        }
    }

    private sealed record TestDomainEvent(string Value) : DomainEvent;
}
