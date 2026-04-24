using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class CadenceBuildArtifactConfiguration : IEntityTypeConfiguration<CadenceBuildArtifact>
{
    public void Configure(EntityTypeBuilder<CadenceBuildArtifact> builder)
    {
        builder.ToTable("CadenceBuildArtifacts");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CadenceBuildJobId);
        builder.HasIndex(x => x.ArtifactType);

        builder.Property(x => x.FilePath).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.Sha256).HasMaxLength(128);

        builder.HasOne(x => x.CadenceBuildJob)
            .WithMany(x => x.Artifacts)
            .HasForeignKey(x => x.CadenceBuildJobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
