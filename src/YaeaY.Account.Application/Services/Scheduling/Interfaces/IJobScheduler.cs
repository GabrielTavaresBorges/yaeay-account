using YaeaY.Account.Application.Services.Scheduling.Models;

namespace YaeaY.Account.Application.Services.Scheduling.Interfaces;

public interface IJobScheduler
{
    Task ScheduleAsync(JobSchedule schedule, CancellationToken cancellationToken = default);

    Task PauseAsync(string jobKey, CancellationToken cancellationToken = default);

    Task ResumeAsync(string jobKey, CancellationToken cancellationToken = default);

    Task RemoveAsync(string jobKey, CancellationToken cancellationToken = default);
}
