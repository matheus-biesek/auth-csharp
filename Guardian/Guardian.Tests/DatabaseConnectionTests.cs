using System.Threading.Tasks;
using Xunit;
using Npgsql;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Guardian.Tests;

public class DatabaseConnectionTests
{
    private string LoadConnectionString()
    {
        // Try environment variable first, then appsettings.Development.json from project
        var env = System.Environment.GetEnvironmentVariable("GUARDIAN_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(env)) return env;

        var configPath = Path.Combine("..", "appsettings.Development.json");
        if (!File.Exists(configPath)) configPath = Path.Combine("..", "Guardian", "appsettings.Development.json");
        var builder = new ConfigurationBuilder();
        if (File.Exists(configPath)) builder.AddJsonFile(configPath, optional: true);
        var config = builder.Build();
        var conn = config.GetConnectionString("GuardianDatabase");
        return conn ?? string.Empty;
    }

    [Fact(Skip = "Integration test that requires a running Postgres instance. Remove Skip to run locally.")]
    public async Task CanOpenPostgresConnection()
    {
        var connString = LoadConnectionString();
        Assert.False(string.IsNullOrWhiteSpace(connString), "Connection string not found. Set GUARDIAN_CONNECTION_STRING or appsettings.Development.json");

        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();
        Assert.Equal(System.Data.ConnectionState.Open, conn.State);
        await conn.CloseAsync();
    }
}
