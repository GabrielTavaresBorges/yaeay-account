using FluentAssertions;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.ValueObjects.Accounts;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Accounts.SuspensionInfoTests;

public class SuspensionInfoIsExpiredTests
{
    [Fact]
    public void IsExpired_WhenSuspensionIsIndefinite_ShouldReturnFalse()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();
        var suspensionInfo = CreateSuspensionInfo(suspendedAt, null);

        // Act

        var isExpired = suspensionInfo.IsExpired(suspendedAt.AddYears(1));

        // Assert

        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenCurrentDateIsBeforeSuspendedUntil_ShouldReturnFalse()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();
        var suspendedUntil = suspendedAt.AddDays(30);
        var suspensionInfo = CreateSuspensionInfo(suspendedAt, suspendedUntil);

        // Act

        var isExpired = suspensionInfo.IsExpired(suspendedUntil.AddTicks(-1));

        // Assert

        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenCurrentDateEqualsSuspendedUntil_ShouldReturnTrue()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();
        var suspendedUntil = suspendedAt.AddDays(30);
        var suspensionInfo = CreateSuspensionInfo(suspendedAt, suspendedUntil);

        // Act

        var isExpired = suspensionInfo.IsExpired(suspendedUntil);

        // Assert

        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenCurrentDateIsAfterSuspendedUntil_ShouldReturnTrue()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();
        var suspendedUntil = suspendedAt.AddDays(30);
        var suspensionInfo = CreateSuspensionInfo(suspendedAt, suspendedUntil);

        // Act

        var isExpired = suspensionInfo.IsExpired(suspendedUntil.AddTicks(1));

        // Assert

        isExpired.Should().BeTrue();
    }

    private static SuspensionInfo CreateSuspensionInfo(
        DateTimeOffset suspendedAt,
        DateTimeOffset? suspendedUntil)
        => SuspensionInfo.Create(
            SuspensionReason.PolicyViolation,
            SuspensionBy.Admin,
            suspendedAt,
            suspendedUntil);

    private static DateTimeOffset CreateSuspendedAt()
        => new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
}
