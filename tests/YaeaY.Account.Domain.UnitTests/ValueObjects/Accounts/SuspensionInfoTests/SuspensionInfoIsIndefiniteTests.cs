using FluentAssertions;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.ValueObjects.Accounts;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Accounts.SuspensionInfoTests;

public class SuspensionInfoIsIndefiniteTests
{
    [Fact]
    public void IsIndefinite_WhenSuspendedUntilIsNull_ShouldReturnTrue()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();
        var suspensionInfo = CreateSuspensionInfo(suspendedAt, null);

        // Act

        var isIndefinite = suspensionInfo.IsIndefinite();

        // Assert

        isIndefinite.Should().BeTrue();
    }

    [Fact]
    public void IsIndefinite_WhenSuspendedUntilHasValue_ShouldReturnFalse()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();
        var suspensionInfo = CreateSuspensionInfo(
            suspendedAt,
            suspendedAt.AddDays(30));

        // Act

        var isIndefinite = suspensionInfo.IsIndefinite();

        // Assert

        isIndefinite.Should().BeFalse();
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
