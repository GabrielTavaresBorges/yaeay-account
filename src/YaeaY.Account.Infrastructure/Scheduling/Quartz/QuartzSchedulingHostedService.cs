using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using YaeaY.Account.Application.Services.Scheduling.Interfaces;
using YaeaY.Account.Application.Services.Scheduling.Models;

namespace YaeaY.Account.Infrastructure.Scheduling.Quartz;

public sealed class QuartzSchedulingHostedService : IHostedService
{
    private readonly IJobScheduler _jobScheduler;
    private readonly OutboxProcessingScheduleOptions _options;

    public QuartzSchedulingHostedService(IJobScheduler jobScheduler, IOptions<OutboxProcessingScheduleOptions> options)
    {
        _jobScheduler = jobScheduler;
        _options = options.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var schedule = new JobSchedule(
            JobKey: QuartzJobKeys.ProcessOutboxMessages,
            Interval: TimeSpan.FromSeconds(_options.IntervalInSeconds),
            StartAt: null,
            EndAt: null,
            IsActive: _options.IsActive);

        var publishSchedule = new JobSchedule(
            JobKey: QuartzJobKeys.PublishOutboxMessages,
            Interval: TimeSpan.FromSeconds(_options.IntervalInSeconds),
            StartAt: null,
            EndAt: null,
            IsActive: _options.IsActive);

        return Task.WhenAll(
            _jobScheduler.ScheduleAsync(schedule, cancellationToken),
            _jobScheduler.ScheduleAsync(publishSchedule, cancellationToken));
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
