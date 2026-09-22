using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atm.Infrastructure.Persistence;

/// <summary>
/// Lets <c>dotnet ef migrations add</c> build the model without the web host.
/// </summary>
internal sealed class AtmDbContextFactory : IDesignTimeDbContextFactory<AtmDbContext>
{
    public AtmDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AtmDbContext>()
            .UseSqlite("Data Source=atm-design-time.db")
            .Options;

        return new AtmDbContext(options);
    }
}
