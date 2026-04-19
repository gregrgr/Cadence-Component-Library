using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class SupplierOfferConfiguration : IEntityTypeConfiguration<SupplierOffer>
{
    public void Configure(EntityTypeBuilder<SupplierOffer> builder)
    {
        builder.ToTable("SupplierOffers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SupplierName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.SupplierSku).HasMaxLength(120);
        builder.Property(x => x.CurrencyCode).HasMaxLength(10);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 6);

        builder.HasOne(x => x.ManufacturerPart)
            .WithMany(x => x.SupplierOffers)
            .HasForeignKey(x => x.ManufacturerPartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
