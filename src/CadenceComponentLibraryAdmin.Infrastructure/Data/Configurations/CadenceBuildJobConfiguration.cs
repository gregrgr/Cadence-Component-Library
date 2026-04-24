using CadenceComponentLibraryAdmin.Domain.Entities;
using CadenceComponentLibraryAdmin.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class CadenceBuildJobConfiguration : IEntityTypeConfiguration<CadenceBuildJob>
{
    public void Configure(EntityTypeBuilder<CadenceBuildJob> builder)
    {
        builder.ToTable("CadenceBuildJobs");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CandidateId);
        builder.HasIndex(x => x.AiDatasheetExtractionId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.JobType);

        builder.Property(x => x.InputJson).IsRequired();
        builder.Property(x => x.ToolName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ToolVersion).HasMaxLength(64);
        builder.Property(x => x.Status).HasDefaultValue(CadenceBuildJobStatus.Pending);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne(x => x.Candidate)
            .WithMany()
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.AiDatasheetExtraction)
            .WithMany(x => x.BuildJobs)
            .HasForeignKey(x => x.AiDatasheetExtractionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
