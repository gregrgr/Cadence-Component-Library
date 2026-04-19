using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class ManufacturerPartConfiguration : IEntityTypeConfiguration<ManufacturerPart>
{
    public void Configure(EntityTypeBuilder<ManufacturerPart> builder)
    {
        builder.ToTable("ManufacturerParts");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.Manufacturer, x.ManufacturerPN }).IsUnique();

        builder.Property(x => x.CompanyPN).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Manufacturer).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ManufacturerPN).HasMaxLength(160).IsRequired();
        builder.Property(x => x.MfgDescription).HasMaxLength(255);
        builder.Property(x => x.PackageCodeRaw).HasMaxLength(120);
        builder.Property(x => x.SourceProvider).HasMaxLength(80);
        builder.Property(x => x.VerifiedBy).HasMaxLength(128);

        builder.HasOne(x => x.CompanyPart)
            .WithMany(x => x.ManufacturerParts)
            .HasPrincipalKey(x => x.CompanyPN)
            .HasForeignKey(x => x.CompanyPN)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
