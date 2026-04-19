using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class LibraryReleaseConfiguration : IEntityTypeConfiguration<LibraryRelease>
{
    public void Configure(EntityTypeBuilder<LibraryRelease> builder)
    {
        builder.ToTable("LibraryReleases");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReleaseName).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ReleasedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ReleaseNote).HasMaxLength(2000);
    }
}
