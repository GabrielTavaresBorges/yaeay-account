using Quartz;
using YaeaY.Account.Application.Services.Scheduling.Interfaces;
using YaeaY.Account.Application.Services.Scheduling.Models;

namespace YaeaY.Account.Infrastructure.Scheduling.Quartz;

public sealed class QuartzJobScheduler : IJobScheduler
{
    private readonly ISchedulerFactory _schedulerFactory;

    public QuartzJobScheduler(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public async Task ScheduleAsync(JobSchedule schedule, CancellationToken cancellationToken = default)
    {
        Validate(schedule);

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = CreateJobKey(schedule.JobKey);
        var triggerKey = CreateTriggerKey(schedule.JobKey);

        if (!await scheduler.CheckExists(jobKey, cancellationToken))
        {
            throw new InvalidOperationException(
                $"Quartz job '{schedule.JobKey}' is not registered.");
        }

        if (!schedule.IsActive)
        {
            if (await scheduler.CheckExists(triggerKey, cancellationToken))
                await scheduler.PauseTrigger(triggerKey, cancellationToken);

            return;
        }

        var triggerBuilder = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .ForJob(jobKey)
            .WithSimpleSchedule(scheduleBuilder => scheduleBuilder
                .WithInterval(schedule.Interval)
                .RepeatForever()
                .WithMisfireHandlingInstructionNextWithRemainingCount());

        triggerBuilder = schedule.StartAt.HasValue
            ? triggerBuilder.StartAt(schedule.StartAt.Value)
            : triggerBuilder.StartNow();

        if (schedule.EndAt.HasValue)
            triggerBuilder.EndAt(schedule.EndAt.Value);

        var trigger = triggerBuilder.Build();

        if (await scheduler.CheckExists(triggerKey, cancellationToken))
            await scheduler.RescheduleJob(triggerKey, trigger, cancellationToken);
        else
            await scheduler.ScheduleJob(trigger, cancellationToken);

        await scheduler.ResumeTrigger(triggerKey, cancellationToken);
    }

    public async Task PauseAsync(string jobKey, CancellationToken cancellationToken = default)
    {
        ValidateJobKey(jobKey);

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        await scheduler.PauseTrigger(CreateTriggerKey(jobKey), cancellationToken);
    }

    public async Task ResumeAsync(string jobKey, CancellationToken cancellationToken = default)
    {
        ValidateJobKey(jobKey);

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        await scheduler.ResumeTrigger(CreateTriggerKey(jobKey), cancellationToken);
    }

    public async Task RemoveAsync(string jobKey, CancellationToken cancellationToken = default)
    {
        ValidateJobKey(jobKey);

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        await scheduler.UnscheduleJob(CreateTriggerKey(jobKey), cancellationToken);
    }

    private static JobKey CreateJobKey(string jobKey) =>
        new(jobKey, QuartzJobKeys.Group);

    private static TriggerKey CreateTriggerKey(string jobKey) =>
        new($"{jobKey}.Trigger", QuartzJobKeys.Group);

    private static void Validate(JobSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ValidateJobKey(schedule.JobKey);

        if (schedule.Interval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(schedule), "Job interval must be positive.");

        if (schedule.StartAt.HasValue &&
            schedule.EndAt.HasValue &&
            schedule.EndAt.Value <= schedule.StartAt.Value)
        {
            throw new ArgumentException("Job end date must be after its start date.", nameof(schedule));
        }
    }

    private static void ValidateJobKey(string jobKey)
    {
        if (string.IsNullOrWhiteSpace(jobKey))
            throw new ArgumentException("Job key is required.", nameof(jobKey));
    }
}
