using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class FootprintVariantConfiguration : IEntityTypeConfiguration<FootprintVariant>
{
    public void Configure(EntityTypeBuilder<FootprintVariant> builder)
    {
        builder.ToTable("FootprintVariants");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.FootprintName).IsUnique();

        builder.Property(x => x.FootprintName).HasMaxLength(140).IsRequired();
        builder.Property(x => x.PackageFamilyCode).HasMaxLength(120).IsRequired();
        builder.Property(x => x.PsmPath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DraPath).HasMaxLength(500);
        builder.Property(x => x.PadstackSet).HasMaxLength(120);
        builder.Property(x => x.StepPath).HasMaxLength(500);
        builder.Property(x => x.VariantType).HasMaxLength(50).IsRequired();

        builder.HasOne(x => x.PackageFamily)
            .WithMany(x => x.FootprintVariants)
            .HasPrincipalKey(x => x.PackageFamilyCode)
            .HasForeignKey(x => x.PackageFamilyCode)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
