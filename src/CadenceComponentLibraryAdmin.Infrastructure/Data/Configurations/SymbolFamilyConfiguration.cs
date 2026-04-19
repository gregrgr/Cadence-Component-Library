using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class SymbolFamilyConfiguration : IEntityTypeConfiguration<SymbolFamily>
{
    public void Configure(EntityTypeBuilder<SymbolFamily> builder)
    {
        builder.ToTable("SymbolFamilies");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.SymbolFamilyCode).IsUnique();

        builder.Property(x => x.SymbolFamilyCode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.SymbolName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.OlbPath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.PartClass).HasMaxLength(40).IsRequired();
        builder.Property(x => x.GateStyle).HasMaxLength(40);
        builder.Property(x => x.PinMapHash).HasMaxLength(64);
    }
}
