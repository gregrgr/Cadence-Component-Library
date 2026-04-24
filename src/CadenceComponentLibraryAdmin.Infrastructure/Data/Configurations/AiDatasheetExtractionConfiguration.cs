using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class AiDatasheetExtractionConfiguration : IEntityTypeConfiguration<AiDatasheetExtraction>
{
    public void Configure(EntityTypeBuilder<AiDatasheetExtraction> builder)
    {
        builder.ToTable("AiDatasheetExtractions");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CandidateId);
        builder.HasIndex(x => x.ExternalImportId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.Manufacturer, x.ManufacturerPartNumber });

        builder.Property(x => x.Manufacturer).HasMaxLength(160).IsRequired();
        builder.Property(x => x.ManufacturerPartNumber).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DatasheetAssetPath).HasMaxLength(1024);
        builder.Property(x => x.ExtractionJson).IsRequired();
        builder.Property(x => x.SymbolSpecJson).IsRequired();
        builder.Property(x => x.FootprintSpecJson).IsRequired();
        builder.Property(x => x.Confidence).HasPrecision(5, 4);
        builder.Property(x => x.Status).HasDefaultValue(Domain.Enums.AiDatasheetExtractionStatus.Draft);
        builder.Property(x => x.ReviewedByUserId).HasMaxLength(450);

        builder.HasOne(x => x.Candidate)
            .WithMany()
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ExternalImport)
            .WithMany()
            .HasForeignKey(x => x.ExternalImportId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
