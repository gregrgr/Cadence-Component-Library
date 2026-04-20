using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class ExternalImportSourceConfiguration : IEntityTypeConfiguration<ExternalImportSource>
{
    public void Configure(EntityTypeBuilder<ExternalImportSource> builder)
    {
        builder.ToTable("ExternalImportSources");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.SourceName).IsUnique();

        builder.Property(x => x.SourceName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasData(new ExternalImportSource
        {
            Id = 1,
            SourceName = "EasyEDA Pro",
            SourceType = ExternalImportSourceType.EasyEdaProSdk,
            Enabled = true,
            Notes = "Seeded import source for the EasyEDA Pro extension staging connector.",
            CreatedAt = new DateTime(2026, 4, 20, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "system",
            IsDeleted = false
        });
    }
}
