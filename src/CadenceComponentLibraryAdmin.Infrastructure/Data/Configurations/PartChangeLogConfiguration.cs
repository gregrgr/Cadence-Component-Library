using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class PartChangeLogConfiguration : IEntityTypeConfiguration<PartChangeLog>
{
    public void Configure(EntityTypeBuilder<PartChangeLog> builder)
    {
        builder.ToTable("PartChangeLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyPN).HasMaxLength(80).IsRequired();
        builder.Property(x => x.OldValue).HasMaxLength(2000);
        builder.Property(x => x.NewValue).HasMaxLength(2000);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.ChangedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ReleaseName).HasMaxLength(40);
    }
}
