using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.SuspensionInfo;
using YaeaY.Account.Domain.ValueObjects.Accounts;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Accounts.SuspensionInfoTests;

public class SuspensionInfoCreateTests
{
    [Fact]
    public void Create_WhenReasonIsUnknown_ShouldThrowDomainException_WithSuspensionInfoErrorsReasonRequired()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();

        // Act

        Action act = () => SuspensionInfo.Create(
            SuspensionReason.Unknown,
            SuspensionBy.Admin,
            suspendedAt);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(SuspensionInfoErrors.ReasonRequired);
    }

    [Fact]
    public void Create_WhenReasonIsNotDefined_ShouldThrowDomainException_WithSuspensionInfoErrorsReasonInvalid()
    {
        // Arrange

        var invalidReason = (SuspensionReason)999;
        var suspendedAt = CreateSuspendedAt();

        // Act

        Action act = () => SuspensionInfo.Create(
            invalidReason,
            SuspensionBy.Admin,
            suspendedAt);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(SuspensionInfoErrors.ReasonInvalid(invalidReason));
    }

    [Fact]
    public void Create_WhenByIsUnknown_ShouldThrowDomainException_WithSuspensionInfoErrorsByRequired()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();

        // Act

        Action act = () => SuspensionInfo.Create(
            SuspensionReason.PolicyViolation,
            SuspensionBy.Unknown,
            suspendedAt);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(SuspensionInfoErrors.ByRequired);
    }

    [Fact]
    public void Create_WhenByIsNotDefined_ShouldThrowDomainException_WithSuspensionInfoErrorsByInvalid()
    {
        // Arrange

        var invalidBy = (SuspensionBy)999;
        var suspendedAt = CreateSuspendedAt();

        // Act

        Action act = () => SuspensionInfo.Create(
            SuspensionReason.PolicyViolation,
            invalidBy,
            suspendedAt);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(SuspensionInfoErrors.ByInvalid(invalidBy));
    }

    [Fact]
    public void Create_WhenSuspendedAtIsDefault_ShouldThrowDomainException_WithSuspensionInfoErrorsSuspendedAtRequired()
    {
        // Act

        Action act = () => SuspensionInfo.Create(
            SuspensionReason.PolicyViolation,
            SuspensionBy.Admin,
            default);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(SuspensionInfoErrors.SuspendedAtRequired);
    }

    [Fact]
    public void Create_WhenNoteIsLongerThanMaximumLength_ShouldThrowDomainException_WithSuspensionInfoErrorsNoteTooLong()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();
        var note = new string('a', 501);

        // Act

        Action act = () => SuspensionInfo.Create(
            SuspensionReason.PolicyViolation,
            SuspensionBy.Admin,
            suspendedAt,
            note: note);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(
            SuspensionInfoErrors.NoteTooLong(note.Length, 500));
    }

    [Fact]
    public void Create_WhenSuspendedUntilEqualsSuspendedAt_ShouldThrowDomainException_WithSuspensionInfoErrorsSuspendedUntilNotAfterSuspendedAt()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();
        var suspendedUntil = suspendedAt;

        // Act

        Action act = () => SuspensionInfo.Create(
            SuspensionReason.PolicyViolation,
            SuspensionBy.Admin,
            suspendedAt,
            suspendedUntil);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(
            SuspensionInfoErrors.SuspendedUntilNotAfterSuspendedAt(
                suspendedAt,
                suspendedUntil));
    }

    [Fact]
    public void Create_WhenSuspendedUntilIsBeforeSuspendedAt_ShouldThrowDomainException_WithSuspensionInfoErrorsSuspendedUntilNotAfterSuspendedAt()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();
        var suspendedUntil = suspendedAt.AddTicks(-1);

        // Act

        Action act = () => SuspensionInfo.Create(
            SuspensionReason.PolicyViolation,
            SuspensionBy.Admin,
            suspendedAt,
            suspendedUntil);

        // Assert

        var exception = act.Should().Throw<DomainException>().Which;
        exception.Error.Should().Be(
            SuspensionInfoErrors.SuspendedUntilNotAfterSuspendedAt(
                suspendedAt,
                suspendedUntil));
    }

    [Fact]
    public void Create_WhenAllDataIsValid_ShouldSucceed()
    {
        // Arrange

        var reason = SuspensionReason.PolicyViolation;
        var by = SuspensionBy.Admin;
        var suspendedAt = CreateSuspendedAt();
        var suspendedUntil = suspendedAt.AddDays(30);
        var note = "Policy violation confirmed.";

        // Act

        var suspensionInfo = SuspensionInfo.Create(
            reason,
            by,
            suspendedAt,
            suspendedUntil,
            note);

        // Assert

        suspensionInfo.Reason.Should().Be(reason);
        suspensionInfo.By.Should().Be(by);
        suspensionInfo.SuspendedAt.Should().Be(suspendedAt);
        suspensionInfo.SuspendedUntil.Should().Be(suspendedUntil);
        suspensionInfo.Note.Should().Be(note);
    }

    [Fact]
    public void Create_WhenSuspendedUntilIsNull_ShouldSucceed_WithIndefiniteSuspension()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();

        // Act

        var suspensionInfo = SuspensionInfo.Create(
            SuspensionReason.FraudRisk,
            SuspensionBy.System,
            suspendedAt);

        // Assert

        suspensionInfo.SuspendedUntil.Should().BeNull();
        suspensionInfo.Note.Should().BeNull();
        suspensionInfo.IsIndefinite().Should().BeTrue();
    }

    [Fact]
    public void Create_WhenNoteContainsWhiteSpaceOnly_ShouldSucceed_WithNullNote()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();

        // Act

        var suspensionInfo = SuspensionInfo.Create(
            SuspensionReason.UserRequested,
            SuspensionBy.User,
            suspendedAt,
            note: "   ");

        // Assert

        suspensionInfo.Note.Should().BeNull();
    }

    [Fact]
    public void Create_WhenNoteHasLeadingOrTrailingSpaces_ShouldSucceed_WithTrimmedNote()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();

        // Act

        var suspensionInfo = SuspensionInfo.Create(
            SuspensionReason.Inactivity,
            SuspensionBy.System,
            suspendedAt,
            note: "  Account inactive.  ");

        // Assert

        suspensionInfo.Note.Should().Be("Account inactive.");
    }

    [Fact]
    public void Create_WhenNoteHasExactlyMaximumLength_ShouldSucceed()
    {
        // Arrange

        var suspendedAt = CreateSuspendedAt();
        var note = new string('a', 500);

        // Act

        var suspensionInfo = SuspensionInfo.Create(
            SuspensionReason.PaymentFailure,
            SuspensionBy.System,
            suspendedAt,
            note: note);

        // Assert

        suspensionInfo.Note.Should().Be(note);
    }

    private static DateTimeOffset CreateSuspendedAt()
        => new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
}
