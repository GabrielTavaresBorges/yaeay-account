using Quartz;
using YaeaY.Account.Application.Services.OutboxMessages.Interfaces;

namespace YaeaY.Account.Infrastructure.Scheduling.Quartz;

[DisallowConcurrentExecution]
public sealed class PublishOutboxMessagesJob(IOutboxMessageProcessor outboxMessageProcessor) : IJob
{
    public Task Execute(IJobExecutionContext context) =>
        outboxMessageProcessor.PublishPendingAsync(context.CancellationToken);
}
