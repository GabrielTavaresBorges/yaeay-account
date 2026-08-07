using System;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using YaeaY.Account.Application.Events.Notifications;
using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Events;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Infrastructure.Data.Context;
using YaeaY.Account.Infrastructure.Data.Persistence;
using YaeaY.Account.Infrastructure.Events.Dispatchers;
using YaeaY.Account.Infrastructure.Events.Publishers;

namespace YaeaY.Account.Infrastructure.UnitTests.Data.PersistenceTests;

public sealed class UnitOfWorkTests
{
    [Fact]
    public async Task CommitAsync_WhenThereAreNoDomainEvents_ShouldSaveChangesWithoutDispatching()
    {
        // Arrange

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        using var context = CreateContext();
        var publisher = new RecordingPublisher();
        var unitOfWork = CreateUnitOfWork(context, publisher);

        // Act

        await unitOfWork.CommitAsync(cancellationToken);

        // Assert

        context.SaveChangesCallCount.Should().Be(1);
        context.SaveChangesCancellationToken.Should().Be(cancellationToken);
        publisher.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task CommitAsync_WhenEntityHasDomainEvent_ShouldSaveDispatchAndClearDomainEvents()
    {
        // Arrange

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var domainEvent = new TestDomainEvent();
        var entity = new TestEntity(domainEvent);

        using var context = CreateContext();
        context.Add(entity);

        var publisher = new RecordingPublisher();
        var unitOfWork = CreateUnitOfWork(context, publisher);

        // Act

        await unitOfWork.CommitAsync(cancellationToken);

        // Assert

        context.SaveChangesCallCount.Should().Be(1);
        context.SaveChangesCancellationToken.Should().Be(cancellationToken);
        entity.DomainEvents.Should().BeEmpty();

        var notification = publisher.Notifications
            .Should()
            .ContainSingle()
            .Which;

        notification.Should().BeOfType<DomainEventNotification<TestDomainEvent>>();

        var domainEventNotification = (DomainEventNotification<TestDomainEvent>)notification;
        domainEventNotification.DomainEvent.Should().BeSameAs(domainEvent);
        publisher.CancellationTokens.Should().ContainSingle().Which.Should().Be(cancellationToken);
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

        var publisher = new RecordingPublisher();
        var unitOfWork = CreateUnitOfWork(context, publisher);

        // Act

        Func<Task> act = () => unitOfWork.CommitAsync();

        // Assert

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.Error.Should().Be(UserErrors.EmailAlreadyInUse);
        exception.Which.InnerException.Should().BeSameAs(dbUpdateException);
        context.SaveChangesCallCount.Should().Be(1);
        publisher.Notifications.Should().BeEmpty();
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

        var publisher = new RecordingPublisher();
        var unitOfWork = CreateUnitOfWork(context, publisher);

        // Act

        Func<Task> act = () => unitOfWork.CommitAsync();

        // Assert

        var exception = await act.Should().ThrowAsync<DbUpdateException>();
        exception.Which.Should().BeSameAs(dbUpdateException);
        context.SaveChangesCallCount.Should().Be(1);
        publisher.Notifications.Should().BeEmpty();
    }

    private static TestAppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
            .Options;

        return new TestAppDbContext(options);
    }

    private static UnitOfWork CreateUnitOfWork(
        AppDbContext context,
        IPublisher publisher)
    {
        var domainEventPublisher = new MediatRDomainEventPublisher(publisher);
        var domainEventDispatcher = new DomainEventDispatcher(domainEventPublisher);

        return new UnitOfWork(context, domainEventDispatcher);
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

    private sealed class RecordingPublisher : IPublisher
    {
        public List<INotification> Notifications { get; } = new();
        public List<CancellationToken> CancellationTokens { get; } = new();

        public Task Publish(
            object notification,
            CancellationToken cancellationToken = default)
        {
            Notifications.Add((INotification)notification);
            CancellationTokens.Add(cancellationToken);

            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            Notifications.Add(notification);
            CancellationTokens.Add(cancellationToken);

            return Task.CompletedTask;
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

    private sealed record TestDomainEvent : DomainEvent;
}
