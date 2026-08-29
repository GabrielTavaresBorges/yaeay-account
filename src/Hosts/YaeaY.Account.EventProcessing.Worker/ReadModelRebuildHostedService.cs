using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YaeaY.Account.Infrastructure.ReadModels;

namespace YaeaY.Account.EventProcessing.Worker;

public sealed class ReadModelRebuildHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<ReadModelRebuildOptions> options,
    ILogger<ReadModelRebuildHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.RebuildMyDataOnStartup)
            return;

        logger.LogWarning("Reconstrução explícita de UserMyData iniciada.");

        using var scope = scopeFactory.CreateScope();
        var rebuilder = scope.ServiceProvider.GetRequiredService<UserMyDataRebuilder>();
        var total = await rebuilder.RebuildAllAsync(stoppingToken);

        logger.LogWarning("Reconstrução explícita de UserMyData concluída para {TotalUsers} usuários.", total);
    }
}
