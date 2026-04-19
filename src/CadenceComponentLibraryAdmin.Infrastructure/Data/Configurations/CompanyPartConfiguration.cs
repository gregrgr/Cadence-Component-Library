using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class CompanyPartConfiguration : IEntityTypeConfiguration<CompanyPart>
{
    public void Configure(EntityTypeBuilder<CompanyPart> builder)
    {
        builder.ToTable("CompanyParts");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.CompanyPN).IsUnique();
        builder.HasIndex(x => x.ApprovalStatus);
        builder.HasIndex(x => x.DefaultFootprintName);
        builder.HasIndex(x => x.PackageFamilyCode);

        builder.Property(x => x.CompanyPN).HasMaxLength(80).IsRequired();
        builder.Property(x => x.PartClass).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ValueNorm).HasMaxLength(80);
        builder.Property(x => x.SymbolFamilyCode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.PackageFamilyCode).HasMaxLength(120).IsRequired();
        builder.Property(x => x.DefaultFootprintName).HasMaxLength(140).IsRequired();
        builder.Property(x => x.AltGroup).HasMaxLength(80);
        builder.Property(x => x.TempRange).HasMaxLength(60);
        builder.Property(x => x.RoHS).HasMaxLength(20);
        builder.Property(x => x.REACHStatus).HasMaxLength(20);
        builder.Property(x => x.DatasheetUrl).HasMaxLength(500);
        builder.Property(x => x.HeightMaxMm).HasPrecision(8, 3);

        builder.HasOne(x => x.SymbolFamily)
            .WithMany()
            .HasPrincipalKey(x => x.SymbolFamilyCode)
            .HasForeignKey(x => x.SymbolFamilyCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PackageFamily)
            .WithMany()
            .HasPrincipalKey(x => x.PackageFamilyCode)
            .HasForeignKey(x => x.PackageFamilyCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DefaultFootprint)
            .WithMany()
            .HasPrincipalKey(x => x.FootprintName)
            .HasForeignKey(x => x.DefaultFootprintName)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
