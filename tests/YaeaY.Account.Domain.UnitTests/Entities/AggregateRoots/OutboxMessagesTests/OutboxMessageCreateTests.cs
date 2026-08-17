using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.AggregateRoots.OutboxMessages;
using YaeaY.Account.Domain.Errors.OutboxMessages;
using YaeaY.Account.Domain.ValueObjects.Events;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.OutboxMessagesTests;

public sealed class OutboxMessageCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenIdIsEmpty_ShouldThrowDomainException()
    {
        // Arrange

        var idInvalid = Guid.Empty;

        var eventType = "UserRegistered";
        var payload = "{}";
        var contentResult = SerializedDomainEvent.Create(eventType, payload);
        var content = contentResult.Value;

        var occurredOnUtc = DateTimeOffset.UtcNow;

        // Action

        Action act = () => OutboxMessage.Create(idInvalid, content, occurredOnUtc);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(OutboxMessageErrors.IdRequired);
    }

    [Fact]
    public void Create_WhenContentIsNull_ShouldThrowDomainException()
    {
        // Arrange

        var id = Guid.NewGuid();
        SerializedDomainEvent contentInvalid = null!;
        var occurredOnUtc = DateTimeOffset.UtcNow;

        // Action

        Action act = () => OutboxMessage.Create(id, contentInvalid, occurredOnUtc);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(OutboxMessageErrors.ContentRequired);
    }

    [Fact]
    public void Create_WhenOccurrenceDateIsDefault_ShouldThrowDomainException()
    {
        // Arrange

        var id = Guid.NewGuid();

        var eventType = "UserRegistered";
        var payload = "{}";
        var contentResult = SerializedDomainEvent.Create(eventType, payload);
        var content = contentResult.Value;

        var occurredOnUtcInvalid = default(DateTimeOffset);

        // Action

        Action act = () => OutboxMessage.Create(id, content, occurredOnUtcInvalid);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(OutboxMessageErrors.OccurredOnUtcRequired);
    }

    // IsSuccess

    [Fact]
    public void Create_WhenDataIsValid_ShouldCreatePendingMessage()
    {
        // Arrange

        var id = Guid.NewGuid();
        var occurredOnUtc = DateTimeOffset.UtcNow;

        var eventType = "UserRegistered";
        var payload = "{}";
        var contentResult = SerializedDomainEvent.Create(eventType, payload);
        var content = contentResult.Value;

        // Action

        var message = OutboxMessage.Create(id, content, occurredOnUtc);

        // Assert

        message.Id.Should().Be(id);
        message.Content.Should().BeSameAs(content);
        message.OccurredOnUtc.Should().Be(occurredOnUtc);
        message.NextAttemptOnUtc.Should().Be(occurredOnUtc);
        message.AttemptCount.Should().Be(0);
        message.IsProcessed.Should().BeFalse();
        message.CanBeProcessed(occurredOnUtc).Should().BeTrue();
    }
}
