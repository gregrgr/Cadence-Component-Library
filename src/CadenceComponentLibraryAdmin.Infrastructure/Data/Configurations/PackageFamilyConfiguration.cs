using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class PackageFamilyConfiguration : IEntityTypeConfiguration<PackageFamily>
{
    public void Configure(EntityTypeBuilder<PackageFamily> builder)
    {
        builder.ToTable("PackageFamilies");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.PackageFamilyCode).IsUnique();
        builder.HasIndex(x => x.PackageSignature).IsUnique();

        builder.Property(x => x.PackageFamilyCode).HasMaxLength(120).IsRequired();
        builder.Property(x => x.MountType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.DensityLevel).HasMaxLength(20);
        builder.Property(x => x.PackageStd).HasMaxLength(80);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.PackageSignature).HasMaxLength(200).IsRequired();
        builder.Property(x => x.BodyLmm).HasPrecision(8, 3);
        builder.Property(x => x.BodyWmm).HasPrecision(8, 3);
        builder.Property(x => x.PitchMm).HasPrecision(8, 3);
        builder.Property(x => x.EPLmm).HasPrecision(8, 3);
        builder.Property(x => x.EPWmm).HasPrecision(8, 3);
    }
}
