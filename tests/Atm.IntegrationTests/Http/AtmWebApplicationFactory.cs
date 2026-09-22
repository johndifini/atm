using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.IntegrationTests.Http;

/// <summary>
/// Hosts the real web application over an isolated SQLite file. A second
/// factory can be pointed at the same file to simulate a restart.
/// </summary>
public sealed class AtmWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly bool _deleteDatabaseOnDispose;
    private readonly string? _environment;
    private readonly Action<IServiceCollection>? _configureServices;

    public AtmWebApplicationFactory(
        string? databasePath = null,
        bool deleteDatabaseOnDispose = true,
        string? environment = null,
        Action<IServiceCollection>? configureServices = null)
    {
        DatabasePath = databasePath ?? NewDatabasePath();
        _deleteDatabaseOnDispose = deleteDatabaseOnDispose;
        _environment = environment;
        _configureServices = configureServices;
    }

    public string DatabasePath { get; }

    public static string NewDatabasePath()
    {
        var directory = Path.Combine(Path.GetTempPath(), "atm-integration-tests");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, $"{Guid.NewGuid():N}.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Host settings are visible to builder.Configuration in Program.cs before
        // the connection string is read; app-configuration callbacks are not.
        builder.UseSetting("ConnectionStrings:Atm", $"Data Source={DatabasePath}");

        if (_environment is not null)
        {
            builder.UseEnvironment(_environment);
        }

        if (_configureServices is not null)
        {
            builder.ConfigureTestServices(_configureServices);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && _deleteDatabaseOnDispose)
        {
            SqliteConnection.ClearAllPools();
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var file = DatabasePath + suffix;
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }
    }
}
