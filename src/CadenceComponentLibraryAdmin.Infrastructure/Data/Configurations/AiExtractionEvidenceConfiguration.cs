using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class AiExtractionEvidenceConfiguration : IEntityTypeConfiguration<AiExtractionEvidence>
{
    public void Configure(EntityTypeBuilder<AiExtractionEvidence> builder)
    {
        builder.ToTable("AiExtractionEvidence");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.AiDatasheetExtractionId);
        builder.HasIndex(x => new { x.AiDatasheetExtractionId, x.FieldPath });

        builder.Property(x => x.FieldPath).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ValueText).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(80);
        builder.Property(x => x.SourceTable).HasMaxLength(255);
        builder.Property(x => x.SourceFigure).HasMaxLength(255);
        builder.Property(x => x.Confidence).HasPrecision(5, 4);
        builder.Property(x => x.ReviewerDecision).HasDefaultValue(AiExtractionReviewerDecision.Pending);
        builder.Property(x => x.ReviewerNote).HasMaxLength(2000);

        builder.HasOne(x => x.AiDatasheetExtraction)
            .WithMany(x => x.EvidenceItems)
            .HasForeignKey(x => x.AiDatasheetExtractionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
