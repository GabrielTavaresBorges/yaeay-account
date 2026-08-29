using Microsoft.Extensions.Configuration;
using Npgsql;

namespace YaeaY.Account.Infrastructure.ReadModels;

public sealed class ReadModelConnectionFactory(IConfiguration configuration)
{
    public NpgsqlConnection CreateConnection()
    {
        var connectionString = configuration.GetConnectionString("ReadConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'ReadConnection' ou 'DefaultConnection' não encontrada.");

        return new NpgsqlConnection(connectionString);
    }
}
