using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class ExternalComponentAssetConfiguration : IEntityTypeConfiguration<ExternalComponentAsset>
{
    public void Configure(EntityTypeBuilder<ExternalComponentAsset> builder)
    {
        builder.ToTable("ExternalComponentAssets");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ExternalComponentImportId);
        builder.HasIndex(x => new { x.SourceName, x.ExternalUuid });
        builder.HasIndex(x => x.AssetType);

        builder.Property(x => x.SourceName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ExternalUuid).HasMaxLength(64);
        builder.Property(x => x.FileName).HasMaxLength(255);
        builder.Property(x => x.OriginalFileName).HasMaxLength(255);
        builder.Property(x => x.ContentType).HasMaxLength(255);
        builder.Property(x => x.StoragePath).HasMaxLength(1000);
        builder.Property(x => x.Url).HasMaxLength(1000);
        builder.Property(x => x.Sha256).HasMaxLength(64);

        builder.HasOne(x => x.ExternalComponentImport)
            .WithMany(x => x.Assets)
            .HasForeignKey(x => x.ExternalComponentImportId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
