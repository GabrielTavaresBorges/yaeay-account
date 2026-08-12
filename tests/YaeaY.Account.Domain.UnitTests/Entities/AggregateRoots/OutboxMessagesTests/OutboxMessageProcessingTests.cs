using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.AggregateRoots.OutboxMessages;
using YaeaY.Account.Domain.Errors.OutboxMessages;
using YaeaY.Account.Domain.ValueObjects.Events;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.OutboxMessagesTests;

public sealed class OutboxMessageProcessingTests
{
    [Fact]
    public void RegisterFailure_WhenDataIsValid_ShouldScheduleNextAttempt()
    {
        var occurredOnUtc = DateTimeOffset.UtcNow;
        var attemptedOnUtc = occurredOnUtc.AddMinutes(1);
        var nextAttemptOnUtc = attemptedOnUtc.AddMinutes(5);
        var message = CreateMessage(occurredOnUtc);

        message.RegisterFailure("  SMTP unavailable  ", attemptedOnUtc, nextAttemptOnUtc);

        message.AttemptCount.Should().Be(1);
        message.LastAttemptOnUtc.Should().Be(attemptedOnUtc);
        message.NextAttemptOnUtc.Should().Be(nextAttemptOnUtc);
        message.LastError.Should().Be("SMTP unavailable");
        message.IsProcessed.Should().BeFalse();
        message.CanBeProcessed(nextAttemptOnUtc.AddTicks(-1)).Should().BeFalse();
        message.CanBeProcessed(nextAttemptOnUtc).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RegisterFailure_WhenFailureIsMissing_ShouldNotChangeState(string? failure)
    {
        var occurredOnUtc = DateTimeOffset.UtcNow;
        var message = CreateMessage(occurredOnUtc);

        Action act = () => message.RegisterFailure(
            failure!,
            occurredOnUtc.AddMinutes(1),
            occurredOnUtc.AddMinutes(2));

        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OutboxMessageErrors.FailureRequired);
        message.AttemptCount.Should().Be(0);
        message.LastAttemptOnUtc.Should().BeNull();
    }

    [Fact]
    public void RegisterFailure_WhenNextAttemptIsNotAfterCurrentAttempt_ShouldNotChangeState()
    {
        var occurredOnUtc = DateTimeOffset.UtcNow;
        var attemptedOnUtc = occurredOnUtc.AddMinutes(1);
        var message = CreateMessage(occurredOnUtc);

        Action act = () => message.RegisterFailure("failure", attemptedOnUtc, attemptedOnUtc);

        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OutboxMessageErrors.NextAttemptNotAfterAttempt);
        message.AttemptCount.Should().Be(0);
        message.LastAttemptOnUtc.Should().BeNull();
    }

    [Fact]
    public void MarkAsProcessed_WhenDateIsValid_ShouldCompleteMessage()
    {
        var occurredOnUtc = DateTimeOffset.UtcNow;
        var processedOnUtc = occurredOnUtc.AddMinutes(1);
        var message = CreateMessage(occurredOnUtc);

        message.MarkAsProcessed(processedOnUtc);

        message.IsProcessed.Should().BeTrue();
        message.ProcessedOnUtc.Should().Be(processedOnUtc);
        message.LastAttemptOnUtc.Should().Be(processedOnUtc);
        message.AttemptCount.Should().Be(1);
        message.LastError.Should().BeNull();
        message.CanBeProcessed(processedOnUtc.AddDays(1)).Should().BeFalse();
    }

    [Fact]
    public void MarkAsProcessed_WhenMessageWasAlreadyProcessed_ShouldThrowDomainException()
    {
        var occurredOnUtc = DateTimeOffset.UtcNow;
        var message = CreateMessage(occurredOnUtc);
        message.MarkAsProcessed(occurredOnUtc.AddMinutes(1));

        Action act = () => message.MarkAsProcessed(occurredOnUtc.AddMinutes(2));

        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OutboxMessageErrors.AlreadyProcessed);
        message.AttemptCount.Should().Be(1);
    }

    [Fact]
    public void RegisterFailure_WhenMessageWasAlreadyProcessed_ShouldThrowDomainException()
    {
        var occurredOnUtc = DateTimeOffset.UtcNow;
        var message = CreateMessage(occurredOnUtc);
        message.MarkAsProcessed(occurredOnUtc.AddMinutes(1));

        Action act = () => message.RegisterFailure(
            "failure",
            occurredOnUtc.AddMinutes(2),
            occurredOnUtc.AddMinutes(3));

        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OutboxMessageErrors.AlreadyProcessed);
        message.AttemptCount.Should().Be(1);
    }

    private static OutboxMessage CreateMessage(DateTimeOffset occurredOnUtc)
    {
        var content = SerializedDomainEvent.Create("UserRegistered", "{}").Value;
        return OutboxMessage.Create(Guid.NewGuid(), content, occurredOnUtc);
    }
}
