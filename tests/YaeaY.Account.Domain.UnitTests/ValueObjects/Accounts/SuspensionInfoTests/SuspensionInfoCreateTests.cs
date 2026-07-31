using FluentAssertions;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.SuspensionInfo;
using YaeaY.Account.Domain.ValueObjects.Accounts;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Accounts.SuspensionInfoTests;

public class SuspensionInfoCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenReasonIsUnknown_ShouldFail_WithSuspensionInfoErrorsReasonRequired()
    {
        // Arrange

        var reasonInvalid = SuspensionReason.Unknown;

        var suspensionBy = SuspensionBy.Admin;
        var suspendedAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset? suspendedUntil = null;
        string? note = null;

        // Act

        var result = SuspensionInfo.Create(
            reasonInvalid,
            suspensionBy,
            suspendedAt,
            suspendedUntil,
            note);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SuspensionInfoErrors.ReasonRequired);
    }

    [Fact]
    public void Create_WhenReasonIsNotDefined_ShouldFail_WithSuspensionInfoErrorsReasonInvalid()
    {
        // Arrange

        var reasonInvalid = (SuspensionReason)999;

        var suspensionBy = SuspensionBy.Admin;
        var suspendedAt = new DateTimeOffset(2026, 1, 2, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset? suspendedUntil = null;
        string? note = null;

        // Act

        var result = SuspensionInfo.Create(
            reasonInvalid,
            suspensionBy,
            suspendedAt,
            suspendedUntil,
            note);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SuspensionInfoErrors.ReasonInvalid(reasonInvalid));
    }

    [Fact]
    public void Create_WhenSuspensionByIsUnknown_ShouldFail_WithSuspensionInfoErrorsByRequired()
    {
        // Arrange

        var suspensionByInvalid = SuspensionBy.Unknown;

        var reason = SuspensionReason.PolicyViolation;
        var suspendedAt = new DateTimeOffset(2026, 1, 3, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset? suspendedUntil = null;
        string? note = null;

        // Act

        var result = SuspensionInfo.Create(
            reason,
            suspensionByInvalid,
            suspendedAt,
            suspendedUntil,
            note);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SuspensionInfoErrors.ByRequired);
    }

    [Fact]
    public void Create_WhenSuspensionByIsNotDefined_ShouldFail_WithSuspensionInfoErrorsByInvalid()
    {
        // Arrange

        var suspensionByInvalid = (SuspensionBy)999;

        var reason = SuspensionReason.PolicyViolation;
        var suspendedAt = new DateTimeOffset(2026, 1, 4, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset? suspendedUntil = null;
        string? note = null;

        // Act

        var result = SuspensionInfo.Create(
            reason,
            suspensionByInvalid,
            suspendedAt,
            suspendedUntil,
            note);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SuspensionInfoErrors.ByInvalid(suspensionByInvalid));
    }

    [Fact]
    public void Create_WhenSuspendedAtIsDefault_ShouldFail_WithSuspensionInfoErrorsSuspendedAtRequired()
    {
        // Arrange

        var suspendedAtInvalid = default(DateTimeOffset);

        var reason = SuspensionReason.PolicyViolation;
        var suspensionBy = SuspensionBy.Admin;
        DateTimeOffset? suspendedUntil = null;
        string? note = null;

        // Act

        var result = SuspensionInfo.Create(
            reason,
            suspensionBy,
            suspendedAtInvalid,
            suspendedUntil,
            note);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SuspensionInfoErrors.SuspendedAtRequired);
    }

    [Fact]
    public void Create_WhenNoteIsLongerThanMaximumLength_ShouldFail_WithSuspensionInfoErrorsNoteTooLong()
    {
        // Arrange

        var noteInvalid = new string('a', 501);

        var reason = SuspensionReason.PolicyViolation;
        var suspensionBy = SuspensionBy.Admin;
        var suspendedAt = new DateTimeOffset(2026, 1, 6, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset? suspendedUntil = null;

        // Act

        var result = SuspensionInfo.Create(
            reason,
            suspensionBy,
            suspendedAt,
            suspendedUntil,
            noteInvalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SuspensionInfoErrors.NoteTooLong(noteInvalid.Length, 500));
    }

    [Fact]
    public void Create_WhenSuspendedUntilEqualsSuspendedAt_ShouldFail_WithSuspensionInfoErrorsSuspendedUntilNotAfterSuspendedAt()
    {
        // Arrange

        var suspendedAt = new DateTimeOffset(2026, 1, 7, 12, 0, 0, TimeSpan.Zero);
        var suspendedUntilInvalid = suspendedAt;

        var reason = SuspensionReason.PolicyViolation;
        var suspensionBy = SuspensionBy.Admin;        
        string? note = null;

        // Act

        var result = SuspensionInfo.Create(
            reason,
            suspensionBy,
            suspendedAt,
            suspendedUntilInvalid,
            note);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(
            SuspensionInfoErrors.SuspendedUntilNotAfterSuspendedAt(suspendedAt, suspendedUntilInvalid));
    }

    [Fact]
    public void Create_WhenSuspendedUntilIsBeforeSuspendedAt_ShouldFail_WithSuspensionInfoErrorsSuspendedUntilNotAfterSuspendedAt()
    {
        // Arrange

        var reason = SuspensionReason.PolicyViolation;
        var suspensionBy = SuspensionBy.Admin;
        var suspendedAt = new DateTimeOffset(2026, 1, 8, 12, 0, 0, TimeSpan.Zero);
        var suspendedUntilInvalid = suspendedAt.AddTicks(-1);
        string? note = null;

        // Act

        var result = SuspensionInfo.Create(
            reason,
            suspensionBy,
            suspendedAt,
            suspendedUntilInvalid,
            note);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SuspensionInfoErrors.SuspendedUntilNotAfterSuspendedAt(suspendedAt, suspendedUntilInvalid));
    }

    // IsSuccess

    [Fact]
    public void Create_WhenAllDataIsValid_ShouldSucceed()
    {
        // Arrange

        var reason = SuspensionReason.PolicyViolation;
        var suspensionBy = SuspensionBy.Admin;
        var suspendedAt = new DateTimeOffset(2026, 1, 9, 12, 0, 0, TimeSpan.Zero);
        var suspendedUntil = suspendedAt.AddDays(30);
        var note = "Policy violation confirmed.";

        // Act

        var result = SuspensionInfo.Create(
            reason,
            suspensionBy,
            suspendedAt,
            suspendedUntil,
            note);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Reason.Should().Be(reason);
        result.Value.SuspensionBy.Should().Be(suspensionBy);
        result.Value.SuspendedAt.Should().Be(suspendedAt);
        result.Value.SuspendedUntil.Should().Be(suspendedUntil);
        result.Value.Note.Should().Be(note);
    }

    [Fact]
    public void Create_WhenSuspendedUntilIsNull_ShouldSucceed_WithIndefiniteSuspension()
    {
        // Arrange

        var reason = SuspensionReason.FraudRisk;
        var suspensionBy = SuspensionBy.System;
        var suspendedAt = new DateTimeOffset(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset? suspendedUntil = null;
        string? note = null;

        // Act

        var result = SuspensionInfo.Create(
            reason,
            suspensionBy,
            suspendedAt,
            suspendedUntil,
            note);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.SuspendedUntil.Should().BeNull();
        result.Value.Note.Should().BeNull();
        result.Value.IsIndefinite().Should().BeTrue();
    }

    [Fact]
    public void Create_WhenNoteContainsWhiteSpaceOnly_ShouldSucceed_WithNullNote()
    {
        // Arrange

        var reason = SuspensionReason.UserRequested;
        var suspensionBy = SuspensionBy.User;
        var suspendedAt = new DateTimeOffset(2026, 1, 11, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset? suspendedUntil = null;
        var note = "   ";

        // Act

        var result = SuspensionInfo.Create(
            reason,
            suspensionBy,
            suspendedAt,
            suspendedUntil,
            note);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Note.Should().BeNull();
    }

    [Fact]
    public void Create_WhenNoteHasLeadingOrTrailingSpaces_ShouldSucceed_WithTrimmedNote()
    {
        // Arrange

        var reason = SuspensionReason.Inactivity;
        var suspensionBy = SuspensionBy.System;
        var suspendedAt = new DateTimeOffset(2026, 1, 12, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset? suspendedUntil = null;
        var note = "  Account inactive.  ";

        // Act

        var result = SuspensionInfo.Create(
            reason,
            suspensionBy,
            suspendedAt,
            suspendedUntil,
            note);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Note.Should().Be("Account inactive.");
    }

    [Fact]
    public void Create_WhenNoteHasExactlyMaximumLength_ShouldSucceed()
    {
        // Arrange

        var reason = SuspensionReason.PaymentFailure;
        var suspensionBy = SuspensionBy.System;
        var suspendedAt = new DateTimeOffset(2026, 1, 13, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset? suspendedUntil = null;
        var note = new string('a', 500);

        // Act

        var result = SuspensionInfo.Create(
            reason,
            suspensionBy,
            suspendedAt,
            suspendedUntil,
            note);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Note.Should().Be(note);
    }
}
