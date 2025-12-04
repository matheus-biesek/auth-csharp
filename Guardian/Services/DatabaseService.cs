using Npgsql;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Guardian.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("GuardianDatabase")
            ?? throw new System.ArgumentNullException("Connection string 'GuardianDatabase' not found.");
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            await conn.CloseAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
