using YaeaY.Account.Domain.Abstraction.Entities;

namespace YaeaY.Account.Domain.Entities.AggregateRoots.Administration;

public sealed class AdministrationAuditEntry : Entity
{
    public Guid AdministratorId { get; private set; }
    public Guid? TargetUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Justification { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }

    private AdministrationAuditEntry() { }

    private AdministrationAuditEntry(Guid administratorId, Guid? targetUserId, string action, string justification, DateTimeOffset occurredAtUtc)
    {
        AdministratorId = administratorId;
        TargetUserId = targetUserId;
        Action = action;
        Justification = justification.Trim();
        OccurredAtUtc = occurredAtUtc;
    }

    public static AdministrationAuditEntry Create(Guid administratorId, Guid? targetUserId, string action, string justification, DateTimeOffset occurredAtUtc)
    {
        if (administratorId == Guid.Empty) throw new ArgumentException("Administrator is required.", nameof(administratorId));
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action is required.", nameof(action));
        if (string.IsNullOrWhiteSpace(justification)) throw new ArgumentException("Justification is required.", nameof(justification));
        if (justification.Trim().Length > 500) throw new ArgumentOutOfRangeException(nameof(justification));
        return new AdministrationAuditEntry(administratorId, targetUserId, action, justification, occurredAtUtc);
    }
}
