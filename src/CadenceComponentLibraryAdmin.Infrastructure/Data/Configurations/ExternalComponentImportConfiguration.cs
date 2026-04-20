using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class ExternalComponentImportConfiguration : IEntityTypeConfiguration<ExternalComponentImport>
{
    public void Configure(EntityTypeBuilder<ExternalComponentImport> builder)
    {
        builder.ToTable("ExternalComponentImports");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.SourceName, x.ExternalDeviceUuid })
            .IsUnique()
            .HasFilter("[ExternalDeviceUuid] IS NOT NULL");
        builder.HasIndex(x => new { x.SourceName, x.LcscId });
        builder.HasIndex(x => new { x.Manufacturer, x.ManufacturerPN });
        builder.HasIndex(x => x.FootprintUuid);
        builder.HasIndex(x => x.SymbolUuid);
        builder.HasIndex(x => x.Model3DUuid);
        builder.HasIndex(x => x.ImportStatus);
        builder.HasIndex(x => x.CandidateId);

        builder.Property(x => x.SourceName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ExternalDeviceUuid).HasMaxLength(64);
        builder.Property(x => x.ExternalLibraryUuid).HasMaxLength(64);
        builder.Property(x => x.SearchKeyword).HasMaxLength(200);
        builder.Property(x => x.LcscId).HasMaxLength(64);
        builder.Property(x => x.ImportKey).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(255);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Manufacturer).HasMaxLength(160);
        builder.Property(x => x.ManufacturerPN).HasMaxLength(200);
        builder.Property(x => x.Supplier).HasMaxLength(160);
        builder.Property(x => x.SupplierId).HasMaxLength(120);
        builder.Property(x => x.SymbolName).HasMaxLength(255);
        builder.Property(x => x.SymbolUuid).HasMaxLength(64);
        builder.Property(x => x.SymbolLibraryUuid).HasMaxLength(64);
        builder.Property(x => x.SymbolType).HasMaxLength(64);
        builder.Property(x => x.FootprintName).HasMaxLength(255);
        builder.Property(x => x.FootprintUuid).HasMaxLength(64);
        builder.Property(x => x.FootprintLibraryUuid).HasMaxLength(64);
        builder.Property(x => x.Model3DName).HasMaxLength(255);
        builder.Property(x => x.Model3DUuid).HasMaxLength(64);
        builder.Property(x => x.Model3DLibraryUuid).HasMaxLength(64);
        builder.Property(x => x.DatasheetUrl).HasMaxLength(1000);
        builder.Property(x => x.ManualUrl).HasMaxLength(1000);
        builder.Property(x => x.StepUrl).HasMaxLength(1000);
        builder.Property(x => x.DuplicateWarning).HasMaxLength(2000);
        builder.Property(x => x.JlcPrice).HasPrecision(18, 6);
        builder.Property(x => x.LcscPrice).HasPrecision(18, 6);

        builder.HasOne(x => x.Candidate)
            .WithMany()
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.FootprintRenderAsset)
            .WithMany()
            .HasForeignKey(x => x.FootprintRenderAssetId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.StepAsset)
            .WithMany()
            .HasForeignKey(x => x.StepAssetId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.DatasheetAsset)
            .WithMany()
            .HasForeignKey(x => x.DatasheetAssetId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ManualAsset)
            .WithMany()
            .HasForeignKey(x => x.ManualAssetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
