using System.Collections.Specialized;
using FluentAssertions;
using Quartz;
using Quartz.Impl;
using YaeaY.Account.Application.Services.Scheduling.Models;
using YaeaY.Account.Infrastructure.Scheduling.Quartz;

namespace YaeaY.Account.Infrastructure.UnitTests.Scheduling.Quartz;

public sealed class QuartzJobSchedulerTests
{
    [Fact]
    public async Task ScheduleAsync_WhenJobIsRegistered_ShouldCreateRecurringTriggerAndSupportLifecycle()
    {
        // Arrange
        var schedulerFactory = CreateSchedulerFactory();
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKey = new JobKey(
            QuartzJobKeys.ProcessOutboxMessages,
            QuartzJobKeys.Group);

        await scheduler.AddJob(
            JobBuilder.Create<TestJob>()
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build(),
            replace: false);

        var jobScheduler = new QuartzJobScheduler(schedulerFactory);
        var schedule = new JobSchedule(
            QuartzJobKeys.ProcessOutboxMessages,
            TimeSpan.FromSeconds(30),
            StartAt: null,
            EndAt: null,
            IsActive: true);

        try
        {
            // Act
            await jobScheduler.ScheduleAsync(schedule);

            // Assert
            var triggerKey = new TriggerKey(
                $"{QuartzJobKeys.ProcessOutboxMessages}.Trigger",
                QuartzJobKeys.Group);
            var trigger = await scheduler.GetTrigger(triggerKey);

            trigger.Should().NotBeNull();
            trigger.Should().BeAssignableTo<ISimpleTrigger>()
                .Which.RepeatInterval.Should().Be(TimeSpan.FromSeconds(30));
            (await scheduler.GetTriggerState(triggerKey))
                .Should().Be(TriggerState.Normal);

            await jobScheduler.PauseAsync(QuartzJobKeys.ProcessOutboxMessages);
            (await scheduler.GetTriggerState(triggerKey))
                .Should().Be(TriggerState.Paused);

            await jobScheduler.ResumeAsync(QuartzJobKeys.ProcessOutboxMessages);
            (await scheduler.GetTriggerState(triggerKey))
                .Should().Be(TriggerState.Normal);

            await jobScheduler.RemoveAsync(QuartzJobKeys.ProcessOutboxMessages);
            (await scheduler.CheckExists(triggerKey)).Should().BeFalse();
        }
        finally
        {
            await scheduler.Shutdown(waitForJobsToComplete: false);
        }
    }

    [Fact]
    public async Task ScheduleAsync_WhenScheduleIsInactive_ShouldNotCreateTrigger()
    {
        // Arrange
        var schedulerFactory = CreateSchedulerFactory();
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKey = new JobKey(
            QuartzJobKeys.ProcessOutboxMessages,
            QuartzJobKeys.Group);

        await scheduler.AddJob(
            JobBuilder.Create<TestJob>()
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build(),
            replace: false);

        var jobScheduler = new QuartzJobScheduler(schedulerFactory);
        var schedule = new JobSchedule(
            QuartzJobKeys.ProcessOutboxMessages,
            TimeSpan.FromSeconds(30),
            StartAt: null,
            EndAt: null,
            IsActive: false);

        try
        {
            // Act
            await jobScheduler.ScheduleAsync(schedule);

            // Assert
            var triggerKey = new TriggerKey(
                $"{QuartzJobKeys.ProcessOutboxMessages}.Trigger",
                QuartzJobKeys.Group);

            (await scheduler.CheckExists(triggerKey)).Should().BeFalse();
        }
        finally
        {
            await scheduler.Shutdown(waitForJobsToComplete: false);
        }
    }

    private static StdSchedulerFactory CreateSchedulerFactory()
    {
        var properties = new NameValueCollection
        {
            ["quartz.scheduler.instanceName"] = $"TestScheduler-{Guid.NewGuid():N}",
            ["quartz.scheduler.instanceId"] = "AUTO",
            ["quartz.threadPool.threadCount"] = "1"
        };

        return new StdSchedulerFactory(properties);
    }

    private sealed class TestJob : IJob
    {
        public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
    }
}
