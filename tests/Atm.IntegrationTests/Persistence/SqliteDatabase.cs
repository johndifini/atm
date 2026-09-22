using Atm.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Atm.IntegrationTests.Persistence;

/// <summary>
/// One isolated SQLite file per test. Each context created from it is
/// independent, which lets tests simulate separate requests, and restarts.
/// </summary>
internal sealed class SqliteDatabase : IDisposable
{
    private readonly string _path;

    public SqliteDatabase()
    {
        var directory = Path.Combine(Path.GetTempPath(), "atm-integration-tests");
        Directory.CreateDirectory(directory);
        _path = Path.Combine(directory, $"{Guid.NewGuid():N}.db");
        ConnectionString = $"Data Source={_path}";
    }

    public string ConnectionString { get; }

    public AtmDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AtmDbContext>()
            .UseSqlite(ConnectionString)
            .Options;
        return new AtmDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await using var db = CreateContext();
        await AtmDatabaseInitializer.InitializeAsync(db);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var file = _path + suffix;
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}
