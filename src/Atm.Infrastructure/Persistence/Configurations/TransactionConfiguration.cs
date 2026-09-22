using Atm.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atm.Infrastructure.Persistence.Configurations;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.OccurredAtUtc)
            .IsRequired();

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(t => t.Amount)
            .IsRequired();

        builder.Property(t => t.SourceAccountId);
        builder.Property(t => t.SourceBalanceAfter);
        builder.Property(t => t.DestinationAccountId);
        builder.Property(t => t.DestinationBalanceAfter);

        builder.HasOne<Domain.Accounts.Account>()
            .WithMany()
            .HasForeignKey(t => t.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Domain.Accounts.Account>()
            .WithMany()
            .HasForeignKey(t => t.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // History is read newest first; Id (a version-7 GUID) breaks ties.
        builder.HasIndex(t => new { t.OccurredAtUtc, t.Id })
            .IsDescending(true, true);
    }
}
