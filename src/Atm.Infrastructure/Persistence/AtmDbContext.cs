using Atm.Domain;
using Atm.Domain.Accounts;
using Atm.Domain.Transactions;
using Atm.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;

namespace Atm.Infrastructure.Persistence;

public sealed class AtmDbContext : DbContext
{
    public AtmDbContext(DbContextOptions<AtmDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Money>()
            .HaveConversion<MoneyConverter>()
            .HavePrecision(18, Money.FractionalDigits);

        configurationBuilder.Properties<AccountId>()
            .HaveConversion<AccountIdConverter>()
            .HaveMaxLength(32);

        // SQLite has no timestamp-with-kind type; every stored instant is UTC and
        // must come back with DateTimeKind.Utc so domain validation still holds.
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtmDbContext).Assembly);
    }
}
