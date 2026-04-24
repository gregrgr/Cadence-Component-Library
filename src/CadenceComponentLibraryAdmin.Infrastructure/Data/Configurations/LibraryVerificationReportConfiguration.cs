using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class LibraryVerificationReportConfiguration : IEntityTypeConfiguration<LibraryVerificationReport>
{
    public void Configure(EntityTypeBuilder<LibraryVerificationReport> builder)
    {
        builder.ToTable("LibraryVerificationReports");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CandidateId);
        builder.HasIndex(x => x.CompanyPartId);
        builder.HasIndex(x => x.AiDatasheetExtractionId);
        builder.HasIndex(x => x.OverallStatus);

        builder.HasOne(x => x.Candidate)
            .WithMany()
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CompanyPart)
            .WithMany()
            .HasForeignKey(x => x.CompanyPartId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.AiDatasheetExtraction)
            .WithMany(x => x.VerificationReports)
            .HasForeignKey(x => x.AiDatasheetExtractionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
