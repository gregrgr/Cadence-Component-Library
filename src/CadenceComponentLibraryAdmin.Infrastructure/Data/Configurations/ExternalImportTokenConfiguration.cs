using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class ExternalImportTokenConfiguration : IEntityTypeConfiguration<ExternalImportToken>
{
    public void Configure(EntityTypeBuilder<ExternalImportToken> builder)
    {
        builder.ToTable("ExternalImportTokens");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.SourceName, x.ExpiresAt });

        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.CreatedByUserEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.SourceName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.RevokedByUserId).HasMaxLength(450);
        builder.Property(x => x.AllowedOrigins).HasMaxLength(2000);
        builder.Property(x => x.Notes).HasMaxLength(2000);
    }
}
