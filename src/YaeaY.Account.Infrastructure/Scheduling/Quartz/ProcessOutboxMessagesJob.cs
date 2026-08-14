using Quartz;
using YaeaY.Account.Application.Services.OutboxMessages.Interfaces;

namespace YaeaY.Account.Infrastructure.Scheduling.Quartz;

[DisallowConcurrentExecution]
public sealed class ProcessOutboxMessagesJob : IJob
{
    private readonly IOutboxMessageProcessor _outboxMessageProcessor;

    public ProcessOutboxMessagesJob(IOutboxMessageProcessor outboxMessageProcessor)
    {
        _outboxMessageProcessor = outboxMessageProcessor;
    }

    public Task Execute(IJobExecutionContext context) =>
        _outboxMessageProcessor.ProcessPendingAsync(context.CancellationToken);
}
