using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.AggregateRoots.OutboxMessages;
using YaeaY.Account.Domain.Errors.OutboxMessages;
using YaeaY.Account.Domain.ValueObjects.Events;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.OutboxMessagesTests;

public sealed class OutboxMessageCreateTests
{
    [Fact]
    public void Create_WhenIdIsEmpty_ShouldThrowDomainException()
    {
        var content = CreateContent();

        Action act = () => OutboxMessage.Create(Guid.Empty, content, DateTimeOffset.UtcNow);

        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OutboxMessageErrors.IdRequired);
    }

    [Fact]
    public void Create_WhenContentIsNull_ShouldThrowDomainException()
    {
        Action act = () => OutboxMessage.Create(Guid.NewGuid(), null!, DateTimeOffset.UtcNow);

        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OutboxMessageErrors.ContentRequired);
    }

    [Fact]
    public void Create_WhenOccurrenceDateIsDefault_ShouldThrowDomainException()
    {
        Action act = () => OutboxMessage.Create(Guid.NewGuid(), CreateContent(), default);

        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OutboxMessageErrors.OccurredOnUtcRequired);
    }

    [Fact]
    public void Create_WhenDataIsValid_ShouldCreatePendingMessage()
    {
        var id = Guid.NewGuid();
        var occurredOnUtc = DateTimeOffset.UtcNow;
        var content = CreateContent();

        var message = OutboxMessage.Create(id, content, occurredOnUtc);

        message.Id.Should().Be(id);
        message.Content.Should().BeSameAs(content);
        message.OccurredOnUtc.Should().Be(occurredOnUtc);
        message.NextAttemptOnUtc.Should().Be(occurredOnUtc);
        message.AttemptCount.Should().Be(0);
        message.IsProcessed.Should().BeFalse();
        message.CanBeProcessed(occurredOnUtc).Should().BeTrue();
    }

    private static SerializedDomainEvent CreateContent() =>
        SerializedDomainEvent.Create("UserRegistered", "{}").Value;
}
