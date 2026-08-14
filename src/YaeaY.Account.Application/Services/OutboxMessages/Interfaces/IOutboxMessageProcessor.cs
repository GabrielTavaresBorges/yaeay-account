namespace YaeaY.Account.Application.Services.OutboxMessages.Interfaces;

public interface IOutboxMessageProcessor
{
    Task ProcessPendingAsync(CancellationToken cancellationToken = default);
}
