using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YaeaY.Account.Infrastructure.ReadModels;
using YaeaY.Account.Infrastructure.ReadModels.Administration;

namespace YaeaY.Account.EventProcessing.Worker;

public sealed class ReadModelRebuildHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<ReadModelRebuildOptions> options,
    ILogger<ReadModelRebuildHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rebuildMyData = options.Value.RebuildMyDataOnStartup;
        var rebuildAdministration = options.Value.RebuildAdministrationOnStartup;

        if (!rebuildMyData && !rebuildAdministration)
        {
            logger.LogInformation("Reconstrução de read models desabilitada nesta inicialização.");
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var total = 0;

        if (rebuildMyData)
        {
            logger.LogWarning("Reconstrução explícita de UserMyData iniciada.");
            var rebuilder = scope.ServiceProvider.GetRequiredService<UserMyDataRebuilder>();
            total = await rebuilder.RebuildAllAsync(stoppingToken);
        }

        if (rebuildAdministration)
        {
            logger.LogWarning("Reconstrução explícita das projeções administrativas iniciada.");
            var administrationRebuilder = scope.ServiceProvider.GetRequiredService<AdministrationReadModelRebuilder>();
            await administrationRebuilder.RebuildAsync(stoppingToken);
        }

        logger.LogWarning("Reconstrução explícita dos read models concluída para {TotalUsers} usuários.", total);
    }
}
