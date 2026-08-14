using FluentAssertions;
using YaeaY.Account.Domain.Events.Users;
using YaeaY.Account.Infrastructure.Messaging.Outbox;

namespace YaeaY.Account.Infrastructure.UnitTests.Messaging.Outbox;

public sealed class JsonDomainEventSerializerTests
{
    [Fact]
    public void Deserialize_WhenEventWasSerialized_ShouldRestoreDomainEvent()
    {
        // Arrange
        var domainEvent = new UserRegisteredDomainEvent(
            Guid.NewGuid(),
            "user@yaeya.com",
            "YaeaY User");
        var serializer = new JsonDomainEventSerializer();
        var serializedEvent = serializer.Serialize(domainEvent);

        // Act
        var result = serializer.Deserialize(serializedEvent);

        // Assert
        var restoredEvent = result.Should()
            .BeOfType<UserRegisteredDomainEvent>()
            .Which;

        restoredEvent.Should().BeEquivalentTo(domainEvent);
    }
}
