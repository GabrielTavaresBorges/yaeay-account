using FluentAssertions;
using YaeaY.Account.Domain.Errors.SerializedDomainEvents;
using YaeaY.Account.Domain.ValueObjects.Events;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Events.SerializedDomainEventTests;

public sealed class SerializedDomainEventCreateTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenEventTypeIsMissing_ShouldFail(string? eventType)
    {
        var result = SerializedDomainEvent.Create(eventType!, "{}");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SerializedDomainEventErrors.EventTypeRequired);
    }

    [Fact]
    public void Create_WhenEventTypeExceedsMaximumLength_ShouldFail()
    {
        var eventType = new string('a', SerializedDomainEvent.EventTypeMaximumLength + 1);

        var result = SerializedDomainEvent.Create(eventType, "{}");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(
            SerializedDomainEventErrors.EventTypeTooLong(
                eventType.Length,
                SerializedDomainEvent.EventTypeMaximumLength));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenPayloadIsMissing_ShouldFail(string? payload)
    {
        var result = SerializedDomainEvent.Create("UserRegistered", payload!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SerializedDomainEventErrors.PayloadRequired);
    }

    [Fact]
    public void Create_WhenPayloadIsNotValidJson_ShouldFail()
    {
        var result = SerializedDomainEvent.Create("UserRegistered", "not-json");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SerializedDomainEventErrors.PayloadInvalid);
    }

    [Fact]
    public void Create_WhenDataIsValid_ShouldCreateNormalizedValueObject()
    {
        var result = SerializedDomainEvent.Create("  UserRegistered  ", "{\"userId\":\"123\"}");

        result.IsSuccess.Should().BeTrue();
        result.Value.EventType.Should().Be("UserRegistered");
        result.Value.Payload.Should().Be("{\"userId\":\"123\"}");
    }
}
