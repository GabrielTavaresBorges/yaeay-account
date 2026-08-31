using FluentAssertions;
using YaeaY.Account.Infrastructure.ReadModels;

namespace YaeaY.Account.Infrastructure.UnitTests.ReadModels;

public sealed class ReadModelFreshnessTests
{
    [Fact]
    public void FromDatabaseTimestamp_WhenPostgreSqlReturnsTimestamp_ShouldReturnUtcDateTimeOffset()
    {
        // Arrange

        var timestamp = new DateTime(2026, 8, 30, 20, 23, 50, DateTimeKind.Unspecified);

        // Act

        var result = ReadModelFreshness.FromDatabaseTimestamp(timestamp);

        // Assert

        result.Should().Be(new DateTimeOffset(2026, 8, 30, 20, 23, 50, TimeSpan.Zero));
    }

    [Fact]
    public void IsCurrent_WhenNoWriteEventExists_ShouldReturnTrue()
    {
        // Arrange

        var projectedThroughUtc = new DateTimeOffset(2026, 8, 30, 12, 0, 0, TimeSpan.Zero);

        // Act

        var isCurrent = ReadModelFreshness.IsCurrent(null, projectedThroughUtc);

        // Assert

        isCurrent.Should().BeTrue();
    }

    [Fact]
    public void IsCurrent_WhenWriteEventIsNewerThanProjection_ShouldReturnFalse()
    {
        // Arrange

        var projectedThroughUtc = new DateTimeOffset(2026, 8, 30, 12, 0, 0, TimeSpan.Zero);
        var latestWriteEventOccurredOnUtc = projectedThroughUtc.AddSeconds(1);

        // Act

        var isCurrent = ReadModelFreshness.IsCurrent(latestWriteEventOccurredOnUtc, projectedThroughUtc);

        // Assert

        isCurrent.Should().BeFalse();
    }

    [Fact]
    public void IsCurrent_WhenWriteEventMatchesProjection_ShouldReturnTrue()
    {
        // Arrange

        var projectedThroughUtc = new DateTimeOffset(2026, 8, 30, 12, 0, 0, TimeSpan.Zero);

        // Act

        var isCurrent = ReadModelFreshness.IsCurrent(projectedThroughUtc, projectedThroughUtc);

        // Assert

        isCurrent.Should().BeTrue();
    }
}
