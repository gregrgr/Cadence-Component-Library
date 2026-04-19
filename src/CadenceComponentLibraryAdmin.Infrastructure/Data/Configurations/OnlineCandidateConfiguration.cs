using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class OnlineCandidateConfiguration : IEntityTypeConfiguration<OnlineCandidate>
{
    public void Configure(EntityTypeBuilder<OnlineCandidate> builder)
    {
        builder.ToTable("OnlineCandidates");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.Manufacturer, x.ManufacturerPN });

        builder.Property(x => x.SourceProvider).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Manufacturer).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ManufacturerPN).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.RawPackageName).HasMaxLength(120);
        builder.Property(x => x.MountType).HasMaxLength(20);
        builder.Property(x => x.RoHS).HasMaxLength(30);
        builder.Property(x => x.ImportNote).HasMaxLength(1000);
        builder.Property(x => x.DatasheetUrl).HasMaxLength(500);
        builder.Property(x => x.PitchMm).HasPrecision(8, 3);
        builder.Property(x => x.BodyLmm).HasPrecision(8, 3);
        builder.Property(x => x.BodyWmm).HasPrecision(8, 3);
        builder.Property(x => x.EPLmm).HasPrecision(8, 3);
        builder.Property(x => x.EPWmm).HasPrecision(8, 3);
    }
}
