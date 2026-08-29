namespace YaeaY.Account.Infrastructure.ReadModels;

public sealed class ReadModelRebuildOptions
{
    public const string SectionName = "ReadModels:Rebuild";

    // Deve ser habilitado explicitamente e removido após a reconstrução inicial.
    public bool RebuildMyDataOnStartup { get; init; }
}
