using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.SuspensionInfo;

namespace YaeaY.Account.Domain.ValueObjects.Accounts;

public sealed record SuspensionInfo
{
    private const int MaximumNoteLength = 500;

    private readonly SuspensionReason _reason;
    private readonly SuspensionBy _by;
    private readonly DateTimeOffset _suspendedAt;
    private readonly DateTimeOffset? _suspendedUntil;
    private readonly string? _note;

    public SuspensionReason Reason => _reason;
    public SuspensionBy By => _by;
    public DateTimeOffset SuspendedAt => _suspendedAt;
    public DateTimeOffset? SuspendedUntil => _suspendedUntil;
    public string? Note => _note;

    private SuspensionInfo(
        SuspensionReason reason,
        SuspensionBy by,
        DateTimeOffset suspendedAt,
        DateTimeOffset? suspendedUntil,
        string? note)
    {
        _reason = reason;
        _by = by;
        _suspendedAt = suspendedAt;
        _suspendedUntil = suspendedUntil;
        _note = note;
    }

    public static SuspensionInfo Create(
        SuspensionReason reason,
        SuspensionBy by,
        DateTimeOffset suspendedAt,
        DateTimeOffset? suspendedUntil = null,
        string? note = null)
    {
        var normalizedNote = NormalizeNote(note);

        Validate(reason, by, suspendedAt, suspendedUntil, normalizedNote);

        return new SuspensionInfo(
            reason: reason,
            by: by,
            suspendedAt: suspendedAt,
            suspendedUntil: suspendedUntil,
            note: normalizedNote
        );
    }

    private static void Validate(
        SuspensionReason reason,
        SuspensionBy by,
        DateTimeOffset suspendedAt,
        DateTimeOffset? suspendedUntil,
        string? note)
    {
        if (reason == SuspensionReason.Unknown)
            throw new DomainException(SuspensionInfoErrors.ReasonRequired);

        if (!Enum.IsDefined(typeof(SuspensionReason), reason))
            throw new DomainException(SuspensionInfoErrors.ReasonInvalid(reason));

        if (by == SuspensionBy.Unknown)
            throw new DomainException(SuspensionInfoErrors.ByRequired);

        if (!Enum.IsDefined(typeof(SuspensionBy), by))
            throw new DomainException(SuspensionInfoErrors.ByInvalid(by));

        if (suspendedAt == default)
            throw new DomainException(SuspensionInfoErrors.SuspendedAtRequired);

        if (note is not null && note.Length > MaximumNoteLength)
            throw new DomainException(
                SuspensionInfoErrors.NoteTooLong(
                    note.Length,
                    MaximumNoteLength));

        if (suspendedUntil.HasValue && suspendedUntil.Value <= suspendedAt)
            throw new DomainException(
                SuspensionInfoErrors.SuspendedUntilNotAfterSuspendedAt(
                    suspendedAt,
                    suspendedUntil.Value));
    }

    private static string? NormalizeNote(string? note)
        => string.IsNullOrWhiteSpace(note) ? null : note.Trim();

    public bool IsExpired(DateTimeOffset nowUtc)
        => SuspendedUntil.HasValue && nowUtc >= SuspendedUntil.Value;

    public bool IsIndefinite()
        => !SuspendedUntil.HasValue;
}
