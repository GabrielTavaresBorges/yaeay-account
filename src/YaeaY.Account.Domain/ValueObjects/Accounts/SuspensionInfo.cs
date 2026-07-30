using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.SuspensionInfo;

namespace YaeaY.Account.Domain.ValueObjects.Accounts;

public sealed record SuspensionInfo
{
    private const int MaximumNoteLength = 500;

    private readonly SuspensionReason _reason;
    private readonly SuspensionBy _suspensionBy;
    private readonly DateTimeOffset _suspendedAt;
    private readonly DateTimeOffset? _suspendedUntil;
    private readonly string? _note;

    public SuspensionReason Reason => _reason;
    public SuspensionBy SuspensionBy => _suspensionBy;
    public DateTimeOffset SuspendedAt => _suspendedAt;
    public DateTimeOffset? SuspendedUntil => _suspendedUntil;
    public string? Note => _note;

    private SuspensionInfo(
        SuspensionReason reason,
        SuspensionBy suspensionBy,
        DateTimeOffset suspendedAt,
        DateTimeOffset? suspendedUntil,
        string? note)
    {
        _reason = reason;
        _suspensionBy = suspensionBy;
        _suspendedAt = suspendedAt;
        _suspendedUntil = suspendedUntil;
        _note = note;
    }

    public static Result<SuspensionInfo> Create(
        SuspensionReason reason,
        SuspensionBy suspensionBy,
        DateTimeOffset suspendedAt,
        DateTimeOffset? suspendedUntil = null,
        string? note = null)
    {
        var normalizedNote = NormalizeNote(note);

        var validateSuspensionInfo = ValidateSuspensionInfo(
            reason,
            suspensionBy,
            suspendedAt,
            suspendedUntil,
            normalizedNote);

        if (validateSuspensionInfo.IsFailure)
            return Result<SuspensionInfo>.Failure(validateSuspensionInfo.Error);

        var suspensionInfo = new SuspensionInfo(
            reason,
            suspensionBy,
            suspendedAt,
            suspendedUntil,
            normalizedNote
        );

        return Result<SuspensionInfo>.Success(suspensionInfo);
    }

    private static Result<bool> ValidateSuspensionInfo(
        SuspensionReason reason,
        SuspensionBy suspensionBy,
        DateTimeOffset suspendedAt,
        DateTimeOffset? suspendedUntil,
        string? note)
    {
        if (reason == SuspensionReason.Unknown)
            return Result<bool>.Failure(SuspensionInfoErrors.ReasonRequired);

        if (!Enum.IsDefined(typeof(SuspensionReason), reason))
            return Result<bool>.Failure(SuspensionInfoErrors.ReasonInvalid(reason));

        if (suspensionBy == SuspensionBy.Unknown)
            return Result<bool>.Failure(SuspensionInfoErrors.ByRequired);

        if (!Enum.IsDefined(typeof(SuspensionBy), suspensionBy))
            return Result<bool>.Failure(SuspensionInfoErrors.ByInvalid(suspensionBy));

        if (suspendedAt == default)
            return Result<bool>.Failure(SuspensionInfoErrors.SuspendedAtRequired);

        if (note is not null && note.Length > MaximumNoteLength)
            return Result<bool>.Failure(SuspensionInfoErrors.NoteTooLong(note.Length, MaximumNoteLength));

        if (suspendedUntil.HasValue && suspendedUntil.Value <= suspendedAt)
            return Result<bool>.Failure(
                SuspensionInfoErrors.SuspendedUntilNotAfterSuspendedAt(suspendedAt, suspendedUntil.Value));

        return Result<bool>.Success(true);
    }

    private static string? NormalizeNote(string? note)
        => string.IsNullOrWhiteSpace(note) ? null : note.Trim();

    public bool IsExpired(DateTimeOffset nowUtc)
        => SuspendedUntil.HasValue && nowUtc >= SuspendedUntil.Value;

    public bool IsIndefinite()
        => !SuspendedUntil.HasValue;
}
