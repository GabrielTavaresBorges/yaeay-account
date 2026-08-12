namespace YaeaY.Account.Application.Services.Scheduling.Models;

public sealed record JobSchedule(
    string JobKey,
    TimeSpan Interval,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    bool IsActive);
