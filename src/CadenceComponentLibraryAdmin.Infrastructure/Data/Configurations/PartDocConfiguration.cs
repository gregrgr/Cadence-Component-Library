using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class PartDocConfiguration : IEntityTypeConfiguration<PartDoc>
{
    public void Configure(EntityTypeBuilder<PartDoc> builder)
    {
        builder.ToTable("PartDocs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyPN).HasMaxLength(80).IsRequired();
        builder.Property(x => x.DocType).HasMaxLength(30).IsRequired();
        builder.Property(x => x.DocUrl).HasMaxLength(500);
        builder.Property(x => x.LocalPath).HasMaxLength(500);
        builder.Property(x => x.VersionTag).HasMaxLength(40);
        builder.Property(x => x.SourceProvider).HasMaxLength(80);

        builder.HasOne(x => x.CompanyPart)
            .WithMany(x => x.Documents)
            .HasPrincipalKey(x => x.CompanyPN)
            .HasForeignKey(x => x.CompanyPN)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
