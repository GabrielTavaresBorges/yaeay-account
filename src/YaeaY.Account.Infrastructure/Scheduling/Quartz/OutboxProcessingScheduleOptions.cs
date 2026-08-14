namespace YaeaY.Account.Infrastructure.Scheduling.Quartz;

public sealed class OutboxProcessingScheduleOptions
{
    public const string SectionName = "Scheduling:OutboxProcessing";

    public bool IsActive { get; init; }
    public int IntervalInSeconds { get; init; }
    public int BatchSize { get; init; }
    public int RetryDelayInSeconds { get; init; }
}
