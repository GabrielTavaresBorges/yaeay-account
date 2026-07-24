using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Domain.ValueObjects.Accounts;

public sealed record SuspensionInfo
{
    public SuspensionReason? Reason { get; private init; }
    public SuspensionBy? By { get; private init; }
    public DateTimeOffset? SuspendedAt { get; private init; }
    public DateTimeOffset? SuspendedUntil { get; private init; }
    public string? Note { get; private init; }    

    private SuspensionInfo(
        SuspensionReason reason,
        SuspensionBy by,
        DateTimeOffset suspendedAt,
        DateTimeOffset? suspendedUntil,
        string? note)
    {
        Reason = reason;
        By = by;
        SuspendedAt = suspendedAt;
        SuspendedUntil = suspendedUntil;
        Note = note;
    }

    public static SuspensionInfo Create(
        SuspensionReason reason,
        SuspensionBy by,
        DateTimeOffset suspendedAt,
        DateTimeOffset? suspendedUntil = null,
        string? note = null)
    {
        Validate(reason, by, suspendedAt, suspendedUntil, note);

        return new SuspensionInfo(
            reason: reason,
            by: by,
            suspendedAt: suspendedAt,
            suspendedUntil: suspendedUntil,
            note: note
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
            throw new DomainException(
                message: "Suspension reason cannot be Unknown.",
                code: "SUSPENSION_REASON_INVALID");

        if (by == SuspensionBy.Unknown)
            throw new DomainException(
                message: "Suspension 'By' cannot be Unknown.",
                code: "SUSPENSION_BY_INVALID");

        // normaliza note
        note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        const int MaxNoteLength = 500;
        if (note is not null && note.Length > MaxNoteLength)
            throw new DomainException(
                message: $"Suspension note is too long. Maximum length is {MaxNoteLength} characters.",
                code: "SUSPENSION_NOTE_TOO_LONG");

        if (suspendedUntil.HasValue && suspendedUntil.Value <= suspendedAt)
            throw new DomainException(
                message: "SuspendedUntil must be greater than SuspendedAt.",
                code: "SUSPENSION_UNTIL_INVALID");
    }

    public bool IsExpired(DateTimeOffset nowUtc)
        => SuspendedUntil.HasValue && nowUtc >= SuspendedUntil.Value;

    public bool IsIndefinite()
        => !SuspendedUntil.HasValue;
}
