using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.AggregateRoots.OutboxMessages;
using YaeaY.Account.Domain.Errors.OutboxMessages;
using YaeaY.Account.Domain.ValueObjects.Events;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.OutboxMessagesTests;

public sealed class OutboxMessageProcessingTests
{
    // IsSuccess

    [Fact]
    public void Processing_WhenFailureIsValid_ShouldScheduleNextAttempt()
    {
        // Arrange

        var occurredOnUtc = DateTimeOffset.UtcNow;
        var attemptedOnUtc = occurredOnUtc.AddMinutes(1);
        var nextAttemptOnUtc = attemptedOnUtc.AddMinutes(5);

        var eventType = "UserRegistered";
        var payload = "{}";
        var contentResult = SerializedDomainEvent.Create(eventType, payload);
        var content = contentResult.Value;

        var id = Guid.NewGuid();
        var message = OutboxMessage.Create(id, content, occurredOnUtc);

        var failure = "  SMTP unavailable  ";

        // Action

        message.RegisterFailure(failure, attemptedOnUtc, nextAttemptOnUtc);

        // Assert

        message.AttemptCount.Should().Be(1);
        message.LastAttemptOnUtc.Should().Be(attemptedOnUtc);
        message.NextAttemptOnUtc.Should().Be(nextAttemptOnUtc);
        message.LastError.Should().Be("SMTP unavailable");
        message.IsProcessed.Should().BeFalse();
        message.CanBeProcessed(nextAttemptOnUtc.AddTicks(-1)).Should().BeFalse();
        message.CanBeProcessed(nextAttemptOnUtc).Should().BeTrue();
    }

    // IsFailure

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Processing_WhenFailureIsMissing_ShouldNotChangeState(string? failure)
    {
        // Arrange

        var occurredOnUtc = DateTimeOffset.UtcNow;

        var eventType = "UserRegistered";
        var payload = "{}";
        var contentResult = SerializedDomainEvent.Create(eventType, payload);
        var content = contentResult.Value;

        var id = Guid.NewGuid();
        var message = OutboxMessage.Create(id, content, occurredOnUtc);

        var attemptedOnUtc = occurredOnUtc.AddMinutes(1);
        var nextAttemptOnUtc = occurredOnUtc.AddMinutes(2);

        // Action

        Action act = () => message.RegisterFailure(
            failure!,
            attemptedOnUtc,
            nextAttemptOnUtc);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(OutboxMessageErrors.FailureRequired);
        message.AttemptCount.Should().Be(0);
        message.LastAttemptOnUtc.Should().BeNull();
    }

    [Fact]
    public void Processing_WhenNextAttemptIsNotAfterCurrentAttempt_ShouldNotChangeState()
    {
        // Arrange

        var occurredOnUtc = DateTimeOffset.UtcNow;
        var attemptedOnUtc = occurredOnUtc.AddMinutes(1);

        var eventType = "UserRegistered";
        var payload = "{}";
        var contentResult = SerializedDomainEvent.Create(eventType, payload);
        var content = contentResult.Value;

        var id = Guid.NewGuid();
        var message = OutboxMessage.Create(id, content, occurredOnUtc);

        var failure = "failure";
        var nextAttemptOnUtcInvalid = attemptedOnUtc;

        // Action

        Action act = () => message.RegisterFailure(
            failure,
            attemptedOnUtc,
            nextAttemptOnUtcInvalid);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(OutboxMessageErrors.NextAttemptNotAfterAttempt);
        message.AttemptCount.Should().Be(0);
        message.LastAttemptOnUtc.Should().BeNull();
    }

    // IsSuccess

    [Fact]
    public void Processing_WhenProcessedDateIsValid_ShouldCompleteMessage()
    {
        // Arrange

        var occurredOnUtc = DateTimeOffset.UtcNow;
        var processedOnUtc = occurredOnUtc.AddMinutes(1);

        var eventType = "UserRegistered";
        var payload = "{}";
        var contentResult = SerializedDomainEvent.Create(eventType, payload);
        var content = contentResult.Value;

        var id = Guid.NewGuid();
        var message = OutboxMessage.Create(id, content, occurredOnUtc);

        // Action

        message.MarkAsProcessed(processedOnUtc);

        // Assert

        message.IsProcessed.Should().BeTrue();
        message.ProcessedOnUtc.Should().Be(processedOnUtc);
        message.LastAttemptOnUtc.Should().Be(processedOnUtc);
        message.AttemptCount.Should().Be(1);
        message.LastError.Should().BeNull();
        message.CanBeProcessed(processedOnUtc.AddDays(1)).Should().BeFalse();
    }

    // IsFailure

    [Fact]
    public void Processing_WhenMarkingAlreadyProcessedMessage_ShouldThrowDomainException()
    {
        // Arrange

        var occurredOnUtc = DateTimeOffset.UtcNow;
        var firstProcessedOnUtc = occurredOnUtc.AddMinutes(1);
        var nextProcessedOnUtc = occurredOnUtc.AddMinutes(2);

        var eventType = "UserRegistered";
        var payload = "{}";
        var contentResult = SerializedDomainEvent.Create(eventType, payload);
        var content = contentResult.Value;

        var id = Guid.NewGuid();
        var message = OutboxMessage.Create(id, content, occurredOnUtc);

        message.MarkAsProcessed(firstProcessedOnUtc);

        // Action

        Action act = () => message.MarkAsProcessed(nextProcessedOnUtc);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(OutboxMessageErrors.AlreadyProcessed);
        message.AttemptCount.Should().Be(1);
    }

    [Fact]
    public void Processing_WhenRegisteringFailureForProcessedMessage_ShouldThrowDomainException()
    {
        // Arrange

        var occurredOnUtc = DateTimeOffset.UtcNow;
        var processedOnUtc = occurredOnUtc.AddMinutes(1);
        var attemptedOnUtc = occurredOnUtc.AddMinutes(2);
        var nextAttemptOnUtc = occurredOnUtc.AddMinutes(3);

        var eventType = "UserRegistered";
        var payload = "{}";
        var contentResult = SerializedDomainEvent.Create(eventType, payload);
        var content = contentResult.Value;

        var id = Guid.NewGuid();
        var message = OutboxMessage.Create(id, content, occurredOnUtc);

        message.MarkAsProcessed(processedOnUtc);

        var failure = "failure";

        // Action

        Action act = () => message.RegisterFailure(
            failure,
            attemptedOnUtc,
            nextAttemptOnUtc);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(OutboxMessageErrors.AlreadyProcessed);
        message.AttemptCount.Should().Be(1);
    }
}
