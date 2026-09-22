using Atm.Application.Ports;
using Atm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SQLite-backed <see cref="AtmDbContext"/> and the persistence
    /// ports it implements. Call <see cref="InitializeAtmDatabaseAsync"/> at startup.
    /// </summary>
    public static IServiceCollection AddAtmPersistence(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<AtmDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static async Task InitializeAtmDatabaseAsync(
        this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AtmDbContext>();
        await AtmDatabaseInitializer.InitializeAsync(db, cancellationToken);
    }
}
