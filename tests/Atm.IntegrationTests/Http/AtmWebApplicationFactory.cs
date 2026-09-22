using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Atm.IntegrationTests.Http;

/// <summary>
/// Hosts the real web application over an isolated SQLite file per factory.
/// </summary>
public sealed class AtmWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath;

    public AtmWebApplicationFactory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "atm-integration-tests");
        Directory.CreateDirectory(directory);
        _databasePath = Path.Combine(directory, $"{Guid.NewGuid():N}.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Host settings are visible to builder.Configuration in Program.cs before
        // the connection string is read; app-configuration callbacks are not.
        builder.UseSetting("ConnectionStrings:Atm", $"Data Source={_databasePath}");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            SqliteConnection.ClearAllPools();
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var file = _databasePath + suffix;
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }
    }
}
