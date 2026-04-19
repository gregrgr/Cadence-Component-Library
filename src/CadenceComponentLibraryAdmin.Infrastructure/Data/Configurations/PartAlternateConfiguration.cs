using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class PartAlternateConfiguration : IEntityTypeConfiguration<PartAlternate>
{
    public void Configure(EntityTypeBuilder<PartAlternate> builder)
    {
        builder.ToTable("PartAlternates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceCompanyPN).HasMaxLength(80).IsRequired();
        builder.Property(x => x.TargetCompanyPN).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.ApprovedBy).HasMaxLength(128);
    }
}
