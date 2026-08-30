namespace YaeaY.Account.Infrastructure.ReadModels;

public static class ReadModelFreshness
{
    public static DateTimeOffset? FromDatabaseTimestamp(DateTime? occurredOnUtc) =>
        occurredOnUtc.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(occurredOnUtc.Value, DateTimeKind.Utc))
            : null;

    public static bool IsCurrent(DateTimeOffset? latestWriteEventOccurredOnUtc, DateTimeOffset projectedThroughUtc) =>
        !latestWriteEventOccurredOnUtc.HasValue ||
        latestWriteEventOccurredOnUtc.Value <= projectedThroughUtc;
}
