using Atm.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atm.Infrastructure.Persistence.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(a => a.Balance)
            .IsRequired();

        // The domain increments Version on every mutation; EF Core includes the
        // originally loaded value in the UPDATE's WHERE clause, so a stale write
        // affects zero rows and surfaces as DbUpdateConcurrencyException.
        builder.Property(a => a.Version)
            .IsRequired()
            .IsConcurrencyToken();
    }
}
